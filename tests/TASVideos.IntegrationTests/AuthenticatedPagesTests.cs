using TASVideos.Data.Entity;

namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class AuthenticatedPagesTests
#pragma warning restore CA1001
{
	private TASVideosWebApplicationFactory _factory = null!;
	private HttpClient _client = null!;

	[TestInitialize]
	public void Setup()
	{
		_factory = new TASVideosWebApplicationFactory();
		_client = _factory.CreateClientWithFreshDatabaseNoRedirects();

		_factory.SeedDatabase(context =>
		{
			context.Users.Add(new User
			{
				UserName = "TestUser",
				NormalizedUserName = "TESTUSER",
				Email = "test@example.com",
				NormalizedEmail = "TEST@EXAMPLE.COM",
				CreateTimestamp = DateTime.UtcNow,
				EmailConfirmed = true
			});
		});
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	[TestMethod]
	public async Task UserFilesUploadPage_WithoutAuth_RedirectsToLogin()
	{
		var response = await _client.GetAsync("/UserFiles/Upload");

		// Should redirect to login page (302) or return unauthorized (401)
		Assert.IsTrue(
			response.StatusCode == HttpStatusCode.Redirect ||
			response.StatusCode == HttpStatusCode.Unauthorized,
			$"Expected redirect or unauthorized, but got {response.StatusCode}");
	}

	[TestMethod]
	public async Task ProfilePage_WithoutAuth_RedirectsToLogin()
	{
		var response = await _client.GetAsync("/Profile/Settings");

		Assert.IsTrue(
			response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Unauthorized,
			$"Expected redirect or unauthorized, but got {response.StatusCode}");
	}

	[TestMethod]
	public async Task AdminOnlyPage_WithoutAuth_RedirectsOrForbidden()
	{
		var response = await _client.GetAsync("/AwardsEditor/2025");

		// Assert
		Assert.IsTrue(
			response.StatusCode is HttpStatusCode.Redirect
				or HttpStatusCode.Unauthorized
				or HttpStatusCode.Forbidden,
			$"Expected redirect, unauthorized, forbidden, or not found, but got {response.StatusCode}");
	}
}
