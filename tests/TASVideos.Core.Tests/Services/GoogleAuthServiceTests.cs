using Microsoft.Extensions.Logging;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class GoogleAuthServiceTests : TestDbBase
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ICacheService _cacheService;
	private readonly AppSettings _appSettings;
	private readonly ILogger<GoogleAuthService> _logger;
	private readonly GoogleAuthService _googleAuthService;

	public GoogleAuthServiceTests()
	{
		_httpClientFactory = Substitute.For<IHttpClientFactory>();
		_cacheService = Substitute.For<ICacheService>();
		_logger = Substitute.For<ILogger<GoogleAuthService>>();

		_appSettings = new AppSettings
		{
			YouTube = new AppSettings.GoogleAuthSettings
			{
				ClientId = "test-client-id",
				ClientSecret = "test-client-secret",
				RefreshToken = "test-refresh-token"
			}
		};

		var httpClient = new HttpClient();
		_httpClientFactory.CreateClient("GoogleAuth").Returns(httpClient);
		_googleAuthService = new GoogleAuthService(_httpClientFactory, _cacheService, _appSettings, _logger);
	}

	[TestMethod]
	public void IsYoutubeEnabled_WhenAllCredentialsProvided_ReturnsTrue()
	{
		var result = _googleAuthService.IsYoutubeEnabled();

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsYoutubeEnabled_WhenClientIdMissing_ReturnsFalse()
	{
		_appSettings.YouTube.ClientId = "";

		var result = _googleAuthService.IsYoutubeEnabled();

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsYoutubeEnabled_WhenClientSecretMissing_ReturnsFalse()
	{
		_appSettings.YouTube.ClientSecret = "";

		var result = _googleAuthService.IsYoutubeEnabled();

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsYoutubeEnabled_WhenRefreshTokenMissing_ReturnsFalse()
	{
		_appSettings.YouTube.RefreshToken = "";

		var result = _googleAuthService.IsYoutubeEnabled();

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsYoutubeEnabled_WhenAllCredentialsMissing_ReturnsFalse()
	{
		_appSettings.YouTube = new AppSettings.GoogleAuthSettings();

		var result = _googleAuthService.IsYoutubeEnabled();

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsYoutubeEnabled_WithWhitespaceOnlyCredentials_ReturnsFalse()
	{
		_appSettings.YouTube = new AppSettings.GoogleAuthSettings
		{
			ClientId = "   ",
			ClientSecret = "\t",
			RefreshToken = "\n"
		};

		var result = _googleAuthService.IsYoutubeEnabled();

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsYoutubeEnabled_WithNullCredentials_ReturnsFalse()
	{
		_appSettings.YouTube = new AppSettings.GoogleAuthSettings
		{
			ClientId = null!,
			ClientSecret = null!,
			RefreshToken = null!
		};

		var result = _googleAuthService.IsYoutubeEnabled();

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsYoutubeEnabled_WithMixedValidAndInvalidCredentials_ReturnsFalse()
	{
		_appSettings.YouTube = new AppSettings.GoogleAuthSettings
		{
			ClientId = "valid-client-id",
			ClientSecret = "",
			RefreshToken = "valid-refresh-token"
		};

		var result = _googleAuthService.IsYoutubeEnabled();

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task GetYoutubeAccessToken_WhenTokenInCache_ReturnsCachedToken()
	{
		const string cachedToken = "cached-access-token";
		_cacheService.TryGetValue("GoogleAuthAccessTokenCacheForYoutube", out Arg.Any<string>())
			.Returns(x =>
			{
				x[1] = cachedToken;
				return true;
			});

		var result = await _googleAuthService.GetYoutubeAccessToken();

		Assert.AreEqual(cachedToken, result);
	}

	[TestMethod]
	public void Constructor_WhenHttpClientFactoryReturnsNull_ThrowsException()
	{
		_httpClientFactory.CreateClient("GoogleAuth").Returns((HttpClient?)null);

		Assert.ThrowsExactly<InvalidOperationException>(() =>
		{
			_ = new GoogleAuthService(_httpClientFactory, _cacheService, _appSettings, _logger);
		});
	}

	[TestMethod]
	public async Task GetYoutubeAccessToken_WhenCacheEmpty_AttemptsToCacheService()
	{
		_cacheService.TryGetValue("GoogleAuthAccessTokenCacheForYoutube", out Arg.Any<string>())
			.Returns(false);

		try
		{
			await _googleAuthService.GetYoutubeAccessToken();
		}
		catch
		{
			// Expected to fail due to network call, but we're testing cache interaction
		}

		_cacheService.Received(1).TryGetValue("GoogleAuthAccessTokenCacheForYoutube", out Arg.Any<string>());
	}

	[TestMethod]
	public async Task GetYoutubeAccessToken_CallsCorrectCacheKey()
	{
		const string expectedCacheKey = "GoogleAuthAccessTokenCacheForYoutube";
		const string cachedToken = "test-token";

		_cacheService.TryGetValue(expectedCacheKey, out Arg.Any<string>())
			.Returns(x =>
			{
				x[1] = cachedToken;
				return true;
			});

		var result = await _googleAuthService.GetYoutubeAccessToken();

		_cacheService.Received(1).TryGetValue(expectedCacheKey, out Arg.Any<string>());
		Assert.AreEqual(cachedToken, result);
	}
}
