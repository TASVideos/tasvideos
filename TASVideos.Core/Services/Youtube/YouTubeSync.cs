using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Services.Youtube.Dtos;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.Extensions;

namespace TASVideos.Core.Services.Youtube
{
	public interface IYoutubeSync
	{
		bool IsYoutubeUrl(string? url);
		string VideoId(string youtubeUrl);

		Task SyncYouTubeVideo(YoutubeVideo video);
		Task UnlistVideo(string url);
		string? ConvertToEmbedLink(string? url);
		Task<IEnumerable<YoutubeVideoResponseItem>> GetPublicInfo(IEnumerable<string> videoIds);
	}

	internal class YouTubeSync : IYoutubeSync
	{
		private const int BatchSize = 50;
		private static readonly string[] BaseTags = { "TAS", "TASVideos", "Tool-Assisted", "Video Game" };
		private readonly HttpClient _client;
		private readonly IGoogleAuthService _googleAuthService;
		private readonly IWikiToTextRenderer _textRenderer;
		private readonly AppSettings _settings;

		public YouTubeSync(
			IHttpClientFactory httpClientFactory,
			IGoogleAuthService googleAuthService,
			IWikiToTextRenderer textRenderer,
			AppSettings settings)
		{
			_client = httpClientFactory.CreateClient(HttpClients.Youtube)
				?? throw new InvalidOperationException($"Unable to initalize {HttpClients.Youtube} client");
			_googleAuthService = googleAuthService;
			_textRenderer = textRenderer;
			_settings = settings;
		}

		public async Task SyncYouTubeVideo(YoutubeVideo video)
		{
			if (!IsYoutubeUrl(video.Url))
			{
				return;
			}

			if (!_googleAuthService.IsYoutubeEnabled())
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

			var descriptionBase = $"This is a tool-assisted speedrun. For more information, see {_settings.BaseUrl}/{video.Id}M";
			if (video.ObsoletedBy.HasValue)
			{
				descriptionBase += $"\n\nThis movie has been obsoleted by {_settings.BaseUrl}/{video.ObsoletedBy.Value}M";
			}

			descriptionBase += $"\nTAS originally published on {video.PublicationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}\n\n";
			var renderedDescription = await _textRenderer.RenderWikiForYoutube(video.WikiPage);

			var requestBody = new VideoUpdateRequest
			{
				VideoId = videoId,
				Snippet = new ()
				{
					Title = $"[TAS] {(video.ObsoletedBy.HasValue ? "[Obsoleted]" : "")} {video.Title}",
					Description = descriptionBase + renderedDescription,
					CategoryId = videoDetails.CategoryId,
					Tags = BaseTags.Concat(video.Tags).ToList()
				}
			}.ToStringContent();

			var response = await _client.PutAsync("videos?part=status,snippet", requestBody);
			response.EnsureSuccessStatusCode();
		}

		public async Task<IEnumerable<YoutubeVideoResponseItem>> GetPublicInfo(IEnumerable<string> videoIds)
		{
			
			if (!_googleAuthService.IsYoutubeEnabled())
			{
				return Enumerable.Empty<YoutubeVideoResponseItem>();
			}

			await SetAccessToken();

			var items = new List<YoutubeVideoResponseItem>();

			var batches = videoIds.Batch(BatchSize);
			foreach (var batch in batches)
			{
				var newItems = await GetBatchPublicInfo(batch.ToList());
				items.AddRange(newItems);
			}

			return items;
		}

		private async Task<IEnumerable<YoutubeVideoResponseItem>> GetBatchPublicInfo(ICollection<string> videoIds)
		{
			if (videoIds.Count > BatchSize)
			{
				throw new InvalidOperationException($"Attempting to batch {videoIds.Count} records, max batch size is {BatchSize}");
			}

			var response = await _client.GetAsync($"videos?id={string.Join(",", videoIds)}&part=snippet");
			if (!response.IsSuccessStatusCode)
			{
				return Enumerable.Empty<YoutubeVideoResponseItem>();
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

			if (!_googleAuthService.IsYoutubeEnabled())
			{
				return;
			}

			var videoId = VideoId(url);
			if (await HasAccessToChannel(videoId) is null)
			{
				return;
			}

			await SetAccessToken();
			var requestBody = new VideoUpdateRequest
			{
				VideoId = videoId,
				Status = new ()
				{
					PrivacyStatus = "unlisted"
				}
			}.ToStringContent();

			var response = await _client.PutAsync("videos?part=status", requestBody);
			response.EnsureSuccessStatusCode();
		}

		public bool IsYoutubeUrl(string? url)
		{
			return !string.IsNullOrWhiteSpace(url) && (url.Contains("youtube.com") || url.Contains("youtu.be"));
		}

		public string? ConvertToEmbedLink(string? url)
		{
			if (!IsYoutubeUrl(url))
			{
				return url;
			}

			var videoId = VideoId(url!);

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
			var accessToken = await _googleAuthService.GetYoutubeAccessToken();
			_client.SetBearerToken(accessToken);
		}

		private async Task<YoutubeVideoSnippetResult?> HasAccessToChannel(string videoId)
		{
			await SetAccessToken();

			// fileDetails require authorization to see, so this serves as a way to determine access
			// there may be a more intended strategy to use
			var result = await _client.GetAsync($"videos?id={videoId}&part=snippet,fileDetails");
			if (result.IsSuccessStatusCode)
			{
				var getResponse = await result.ReadAsync<YoutubeGetResponse>();
				return getResponse.Items.First().Snippet;
			}

			return null;
		}
	}

	public record YoutubeVideo(
		int Id,
		DateTime PublicationDate,
		string Url,
		string Title,
		WikiPage WikiPage,
		string SystemCode,
		IEnumerable<string> Authors,
		string? SearchKey,
		int? ObsoletedBy)
	{
		public IEnumerable<string> Tags
		{
			get
			{
				var tags = new[] { SystemCode }
					.Concat(Authors);

				if (!string.IsNullOrWhiteSpace(SearchKey))
				{
					tags = tags.Concat(SearchKey.SplitWithEmpty("-"));
				}

				tags = tags.Select(t => t.ToLower()).Distinct();
				return tags;
			}
		} 
	}
}
