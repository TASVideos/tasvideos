using System;
using System.Net.Http;
using System.Threading.Tasks;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Services.Youtube.Dtos;

namespace TASVideos.Core.Services.Youtube
{
	public interface IYoutubeSync
	{
		bool IsYoutubeUrl(string url);
		Task SyncYouTubeVideos(string url);
		Task UnlistVideo(string url);
	}

	public class YouTubeSync : IYoutubeSync
	{
		private readonly HttpClient _client;
		private readonly IGoogleAuthService _googleAuthService;

		public YouTubeSync(IHttpClientFactory httpClientFactory, IGoogleAuthService googleAuthService)
		{
			_client = httpClientFactory.CreateClient(HttpClients.Youtube)
				?? throw new InvalidOperationException($"Unable to initalize {HttpClients.Youtube} client");
			_googleAuthService = googleAuthService;
		}

		public async Task SyncYouTubeVideos(string url)
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
			if (!await HasAccessToChannel(videoId))
			{
				return;
			}

			await SetAccessToken();
			var requestBody = new VideoUpdateRequest
			{
				VideoId = videoId
			}.ToStringContent();

			var response = await _client.PutAsync("videos?part=status", requestBody);
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
			if (!await HasAccessToChannel(videoId))
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
			return !string.IsNullOrWhiteSpace(url) && url.Contains("youtube.com");
		}

		private static string VideoId(string youtubeUrl)
		{
			return youtubeUrl[(youtubeUrl.IndexOf("v=") + 2)..];
		}

		private async Task SetAccessToken()
		{
			var accessToken = await _googleAuthService.GetAccessToken();
			_client.SetBearerToken(accessToken);
		}

		private async Task<bool> HasAccessToChannel(string videoId)
		{
			await SetAccessToken();

			// fileDetails require authorization to see, so this serves as a way to determine access
			// there may be a more intended strategy to use
			var result = await _client.GetAsync($"videos?id={videoId}&part=fileDetails");
			return result.IsSuccessStatusCode;
		}
	}
}
