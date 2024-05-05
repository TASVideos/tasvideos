using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Services.Wiki;
using TASVideos.Core.Services.Youtube.Dtos;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.Youtube;

public interface IYoutubeSync
{
	bool IsYoutubeUrl(string? url);
	string VideoId(string youtubeUrl);

	Task SyncYouTubeVideo(YoutubeVideo video);
	Task UnlistVideo(string url);
	string? ConvertToEmbedLink(string? url);
	Task<IEnumerable<YoutubeVideoResponseItem>> GetPublicInfo(IEnumerable<string> videoIds);
}

internal class YouTubeSync(
	IHttpClientFactory httpClientFactory,
	IGoogleAuthService googleAuthService,
	IWikiToTextRenderer textRenderer,
	AppSettings settings,
	ILogger<YouTubeSync> logger)
	: IYoutubeSync
{
	private const int YoutubeTitleMaxLength = 100;
	private const int BatchSize = 50;
	private static readonly string[] BaseTags = ["TAS", "TASVideos", "Tool-Assisted", "Video Game"];
	private readonly HttpClient _client = httpClientFactory.CreateClient(HttpClients.Youtube)
		?? throw new InvalidOperationException($"Unable to initialize {HttpClients.Youtube} client");

	public async Task SyncYouTubeVideo(YoutubeVideo video)
	{
		if (!IsYoutubeUrl(video.Url))
		{
			return;
		}

		if (!googleAuthService.IsYoutubeEnabled())
		{
			return;
		}

		var videoId = VideoId(video.Url);
		var videoDetails = await HasAccessToChannel(videoId);
		if (videoDetails is null)
		{
			return;
		}

		await SetAccessToken();

		var descriptionBase = $"This is a tool-assisted speedrun. For more information, see {settings.BaseUrl}/{video.Id}M";
		if (video.ObsoletedBy.HasValue)
		{
			descriptionBase += $"\n\nThis movie has been obsoleted by {settings.BaseUrl}/{video.ObsoletedBy.Value}M";
		}

		descriptionBase += $"\nTAS originally published on {video.PublicationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}\n\n";
		var renderedDescription = await textRenderer.RenderWikiForYoutube(video.WikiPage);

		const string hashTags = "\n\n#tas #tasvideos #toolassisted #toolassistedspeedrun #speedrun";

		var obsoleteStr = video.ObsoletedBy.HasValue ? "[Obsoleted] " : "";
		var displayStr = !string.IsNullOrWhiteSpace(video.UrlDisplayName) ? $"[{video.UrlDisplayName}] " : "";
		string title = $"[TAS] {obsoleteStr}{displayStr}{video.Title}";
		string description = descriptionBase + renderedDescription + hashTags;
		if (title.Length > YoutubeTitleMaxLength)
		{
			description = title + "\n" + descriptionBase + renderedDescription + hashTags;
			title = title.CapAndEllipse(YoutubeTitleMaxLength);
		}

		var updateRequest = new VideoUpdateRequest
		{
			VideoId = videoId,
			Snippet = new()
			{
				Title = title.FormatForYouTube(),
				Description = description.FormatForYouTube(),
				CategoryId = videoDetails.CategoryId,
				Tags = [.. BaseTags, .. video.Tags]
			}
		};

		var response = await _client.PutAsync("videos?part=status,snippet", updateRequest.ToStringContent());
		if (!response.IsSuccessStatusCode)
		{
			logger.LogError(
				"[{timestamp}] An error occurred syncing data to Youtube. Request: {request} Response: {response}",
				DateTime.UtcNow,
				JsonSerializer.Serialize(updateRequest),
				await response.Content.ReadAsStringAsync());
		}
	}

	public async Task<IEnumerable<YoutubeVideoResponseItem>> GetPublicInfo(IEnumerable<string> videoIds)
	{
		if (!googleAuthService.IsYoutubeEnabled())
		{
			return [];
		}

		await SetAccessToken();

		var items = new List<YoutubeVideoResponseItem>();

		var batches = videoIds.Chunk(BatchSize);
		foreach (var batch in batches)
		{
			var newItems = await GetBatchPublicInfo([.. batch]);
			items.AddRange(newItems);
		}

		return items;
	}

	private async Task<IEnumerable<YoutubeVideoResponseItem>> GetBatchPublicInfo(IReadOnlyCollection<string> videoIds)
	{
		if (videoIds.Count > BatchSize)
		{
			throw new InvalidOperationException($"Attempting to batch {videoIds.Count} records, max batch size is {BatchSize}");
		}

		var response = await _client.GetAsync($"videos?id={string.Join(",", videoIds)}&part=snippet");
		if (!response.IsSuccessStatusCode)
		{
			return [];
		}

		var data = await response.ReadAsync<YoutubeVideoResponse>();
		return data.Items;
	}

	public async Task UnlistVideo(string url)
	{
		if (!IsYoutubeUrl(url))
		{
			return;
		}

		if (!googleAuthService.IsYoutubeEnabled())
		{
			return;
		}

		var videoId = VideoId(url);
		if (await HasAccessToChannel(videoId) is null)
		{
			return;
		}

		await SetAccessToken();

		var unlistRequest = new UnlistRequest
		{
			VideoId = videoId,
			Status = new()
			{
				PrivacyStatus = "unlisted"
			}
		};

		var response = await _client.PutAsync("videos?part=status", unlistRequest.ToStringContent());

		if (!response.IsSuccessStatusCode)
		{
			logger.LogError(
				"{timestamp} An error occurred sending a request to YouTube. Request: {request} Response: {response}",
				DateTime.UtcNow,
				JsonSerializer.Serialize(unlistRequest),
				await response.Content.ReadAsStringAsync());
		}
	}

	public bool IsYoutubeUrl(string? url)
	{
		return !string.IsNullOrWhiteSpace(url) && (url.Contains("youtube.com") || url.Contains("youtu.be"));
	}

	public string? ConvertToEmbedLink(string? url)
	{
		if (!IsYoutubeUrl(url))
		{
			return url.NullIfWhitespace();
		}

		if (url!.Contains("/embed"))
		{
			return url;
		}

		var videoId = VideoId(url);

		if (string.IsNullOrWhiteSpace(videoId))
		{
			return url;
		}

		return $"https://www.youtube.com/embed/{videoId}";
	}

	public string VideoId(string youtubeUrl)
	{
		if (string.IsNullOrWhiteSpace(youtubeUrl))
		{
			return "";
		}

		if (youtubeUrl.Contains("https://youtu.be"))
		{
			return youtubeUrl.SplitWithEmpty("/").Last();
		}

		var result = youtubeUrl[(youtubeUrl.IndexOf("v=") + 2)..];

		if (!string.IsNullOrWhiteSpace(result))
		{
			// Account for anchors
			result = result.SplitWithEmpty("#")[0];

			// Account for additional query string params
			result = result.SplitWithEmpty("&")[0].TrimEnd('?');
		}

		return result;
	}

	private async Task SetAccessToken()
	{
		var accessToken = await googleAuthService.GetYoutubeAccessToken();
		_client.SetBearerToken(accessToken);
	}

	private async Task<YoutubeVideoSnippetResult?> HasAccessToChannel(string videoId)
	{
		await SetAccessToken();

		// fileDetails require authorization to see, so this serves as a way to determine access
		// there may be a more intended strategy to use
		var result = await _client.GetAsync($"videos?id={videoId}&part=snippet,fileDetails");
		if (!result.IsSuccessStatusCode)
		{
			logger.LogError(
				"{timestamp} Unable to request data for video {videoId} from YouTube. Response: {response}",
				DateTime.UtcNow,
				videoId,
				await result.Content.ReadAsStringAsync());
			return null;
		}

		var getResponse = await result.ReadAsync<YoutubeGetResponse>();
		return getResponse.Items.First().Snippet;
	}
}

public record YoutubeVideo(
	int Id,
	DateTime PublicationDate,
	string Url,
	string? UrlDisplayName,
	string Title,
	IWikiPage WikiPage,
	string SystemCode,
	IEnumerable<string> Authors,
	int? ObsoletedBy)
{
	public IEnumerable<string> Tags =>
		new[] { SystemCode }
			.Concat(Authors)
			.Select(t => t.ToLower())
			.Distinct();
}
