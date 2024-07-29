using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public sealed class SignInManagerTests : TestDbBase
{
	private readonly SignInManager _signInManager;

	public SignInManagerTests()
	{
		var identityOptions = Substitute.For<IOptions<IdentityOptions>>();
		var userManager = new UserManager(
			_db,
			new TestCache(),
			Substitute.For<IPointsService>(),
			Substitute.For<ITASVideoAgent>(),
			Substitute.For<IWikiPages>(),
			Substitute.For<IUserStore<User>>(),
			identityOptions,
			Substitute.For<IPasswordHasher<User>>(),
			Substitute.For<IEnumerable<IUserValidator<User>>>(),
			Substitute.For<IEnumerable<IPasswordValidator<User>>>(),
			Substitute.For<ILookupNormalizer>(),
			new IdentityErrorDescriber(),
			Substitute.For<IServiceProvider>(),
			Substitute.For<ILogger<UserManager<User>>>());

		_signInManager = new SignInManager(
			_db,
			userManager,
			Substitute.For<IHttpContextAccessor>(),
			Substitute.For<IUserClaimsPrincipalFactory<User>>(),
			identityOptions,
			Substitute.For<ILogger<SignInManager<User>>>(),
			Substitute.For<IAuthenticationSchemeProvider>(),
			Substitute.For<IUserConfirmation<User>>());
	}

	[TestMethod]
	[DataRow(null, null, null, false)]
	[DataRow("test", "", "test", false)]
	[DataRow("test", "", "test123", true)]
	[DataRow("test", "test123@example.com", "test123", false)]
	public void IsPasswordAllowed_Tests(string userName, string email, string password, bool expected)
	{
		var actual = _signInManager.IsPasswordAllowed(userName, email, password);
		Assert.AreEqual(expected, actual);
	}
}
