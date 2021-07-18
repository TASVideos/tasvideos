using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Services.Youtube.Dtos;
using TASVideos.Extensions;

namespace TASVideos.Core.Services.Youtube
{
	public interface IYoutubeSync
	{
		bool IsYoutubeUrl(string url);
		Task SyncYouTubeVideo(YoutubeVideo video);
		Task UnlistVideo(string url);
	}

	public class YouTubeSync : IYoutubeSync
	{
		private static readonly string[] BaseTags = { "TAS", "TASVideos", "Tool-Assisted", "Video Game" };

		private readonly HttpClient _client;
		private readonly IGoogleAuthService _googleAuthService;

		public YouTubeSync(IHttpClientFactory httpClientFactory, IGoogleAuthService googleAuthService)
		{
			_client = httpClientFactory.CreateClient(HttpClients.Youtube)
				?? throw new InvalidOperationException($"Unable to initalize {HttpClients.Youtube} client");
			_googleAuthService = googleAuthService;
		}

		public async Task SyncYouTubeVideo(YoutubeVideo video)
		{
			if (!IsYoutubeUrl(video.Url))
			{
				return;
			}

			if (!_googleAuthService.IsEnabled())
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
			var requestBody = new VideoUpdateRequest
			{
				VideoId = videoId,
				Snippet = new ()
				{
					Title = video.Title,
					Description = video.Description,
					CategoryId = videoDetails.CategoryId,
					Tags = BaseTags.Concat(video.Tags).ToList()
				}
			}.ToStringContent();

			var response = await _client.PutAsync("videos?part=status,snippet", requestBody);
			response.EnsureSuccessStatusCode();
		}

		public async Task UnlistVideo(string url)
		{
			if (!IsYoutubeUrl(url))
			{
				return;
			}

			if (!_googleAuthService.IsEnabled())
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

		public bool IsYoutubeUrl(string url)
		{
			return !string.IsNullOrWhiteSpace(url) && (url.Contains("youtube.com") || url.Contains("youtu.be"));
		}

		internal static string VideoId(string youtubeUrl)
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
			var accessToken = await _googleAuthService.GetAccessToken();
			_client.SetBearerToken(accessToken);
		}

		private async Task<YoutubeVideoSnippet?> HasAccessToChannel(string videoId)
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

	public record YoutubeVideo(string Url, string Title, string Description, IEnumerable<string> Tags);
}
