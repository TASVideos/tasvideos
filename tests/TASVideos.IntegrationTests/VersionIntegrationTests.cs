using System.Text.RegularExpressions;

namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class VersionIntegrationTests
#pragma warning restore CA1001
{
	private TASVideosWebApplicationFactory _factory = null!;
	private HttpClient _client = null!;

	[TestInitialize]
	public void Setup()
	{
		_factory = new TASVideosWebApplicationFactory();
		_client = _factory.CreateClientWithFollowRedirects();
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	[TestMethod]
	public async Task HomePage_VersionLinkPointsToCorrectCommit()
	{
		var response = await _client.GetAsync("/");

		response.EnsureSuccessStatusCode("Home page should load successfully");

		// Check that the version link href contains a valid commit SHA
		var footerLink = await response.QuerySelectorAsync("a[href*='github.com/TASVideos/tasvideos/commits/']");
		Assert.IsNotNull(footerLink, "Version link should be present in footer");

		var href = footerLink.GetAttribute("href");
		Assert.IsNotNull(href, "Version link should have an href attribute");

		var footerText = footerLink.TextContent;
		Assert.IsTrue(
			footerText.Contains("TASVideos v"),
			$"Footer should contain 'TASVideos v' but got: {footerText}");

		// Extract SHA from URL (e.g., "https://github.com/TASVideos/tasvideos/commits/e2f009c7e95aa93b732b9c7d59c00fcd30fbd0b4")
		var shaMatch = Regex.Match(href, "/commits/([a-f0-9]+)$");
		Assert.IsTrue(shaMatch.Success, $"Could not extract SHA from link href: {href}");

		var sha = shaMatch.Groups[1].Value;
		Assert.IsTrue(sha.Length >= 7, $"SHA should be at least 7 characters, but got: {sha} (length: {sha.Length})");
		Assert.IsTrue(
			sha.All(c => char.IsDigit(c) || c is >= 'a' and <= 'f'),
			$"SHA should only contain hex characters, but got: {sha}");

		// Extract version number from footer text (e.g., "© 2025 - TASVideos v2.6-01c52d3")
		var versionMatch = Regex.Match(footerText, @"TASVideos v(\d+)\.(\d+)-([a-f0-9]+)");
		Assert.IsTrue(versionMatch.Success, $"Could not extract version number from footer text: {footerText}");

		var majorVersion = int.Parse(versionMatch.Groups[1].Value);
		var minorVersion = int.Parse(versionMatch.Groups[2].Value);
		var shortSha = versionMatch.Groups[3].Value;

		Assert.IsTrue(majorVersion > 0, $"Major version should be greater than 0, but got: {majorVersion}");
		Assert.IsTrue(minorVersion >= 0, $"Minor version should be >= 0, but got: {minorVersion}");
		Assert.IsTrue(shortSha.Length >= 7, $"Short SHA should be at least 7 characters, but got: {shortSha} (length: {shortSha.Length})");
	}
}
