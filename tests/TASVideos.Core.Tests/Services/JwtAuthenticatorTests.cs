using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class JwtAuthenticatorTests : TestDbBase
{
	private readonly ISignInManager _signInManager;
	private readonly IUserManager _userManager;
	private readonly AppSettings _appSettings;
	private readonly JwtAuthenticator _jwtAuthenticator;

	public JwtAuthenticatorTests()
	{
		_signInManager = Substitute.For<ISignInManager>();
		_userManager = Substitute.For<IUserManager>();
		_appSettings = new AppSettings
		{
			Jwt = new AppSettings.JwtSettings
			{
				SecretKey = "this-is-a-test-secret-key-that-is-long-enough-for-hmac-sha256",
				ExpiresInMinutes = 60
			}
		};

		_jwtAuthenticator = new JwtAuthenticator(_signInManager, _userManager, _appSettings);
	}

	[TestMethod]
	public async Task Authenticate_ValidCredentials_ReturnsJwtToken()
	{
		const string username = "TestUser";
		const string password = "TestPassword";
		const int userId = 123;
		const string email = "test@example.com";
		const string customClaimValue = "custom-value";
		var user = new User
		{
			Id = userId,
			UserName = username,
			Email = email
		};

		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new(ClaimTypes.Name, user.UserName),
			new(ClaimTypes.Email, user.Email),
			new("custom-claim", customClaimValue)
		};

		_userManager.GetClaims(user).Returns(claims);
		_signInManager.SignIn(username, password).Returns((SignInResult.Success, user, false));

		var result = await _jwtAuthenticator.Authenticate(username, password);

		Assert.IsNotNull(result);
		Assert.IsTrue(result.Length > 0);

		var tokenHandler = new JsonWebTokenHandler();
		var key = Encoding.ASCII.GetBytes(_appSettings.Jwt.SecretKey);
		var validationParameters = new TokenValidationParameters
		{
			IssuerSigningKey = new SymmetricSecurityKey(key),
			ValidateIssuer = false,
			ValidateAudience = false,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};

		var validationResult = await tokenHandler.ValidateTokenAsync(result, validationParameters);
		Assert.IsTrue(validationResult.IsValid);

		var jsonWebToken = new JsonWebToken(result);
		Assert.IsTrue(jsonWebToken.Claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString()));
		Assert.IsTrue(jsonWebToken.Claims.Any(c => c.Type == ClaimTypes.Name && c.Value == username));
		Assert.IsTrue(jsonWebToken.Claims.Any(c => c.Type == ClaimTypes.Email && c.Value == email));
		Assert.IsTrue(jsonWebToken.Claims.Any(c => c.Type == "custom-claim" && c.Value == customClaimValue));
	}

	[TestMethod]
	public async Task Authenticate_InvalidCredentials_ReturnsEmptyString()
	{
		const string username = "InvalidUser";
		const string password = "WrongPassword";
		_signInManager.SignIn(username, password).Returns((SignInResult.Failed, null, false));

		var result = await _jwtAuthenticator.Authenticate(username, password);

		Assert.AreEqual("", result);
	}

	[TestMethod]
	public async Task Authenticate_LockedOutUser_ReturnsEmptyString()
	{
		const string username = "LockedUser";
		const string password = "TestPassword";
		_signInManager.SignIn(username, password).Returns((SignInResult.LockedOut, null, false));

		var result = await _jwtAuthenticator.Authenticate(username, password);

		Assert.AreEqual("", result);
	}

	[TestMethod]
	public async Task Authenticate_SignInSucceedsButUserIsNull_ReturnsEmptyString()
	{
		const string username = "TestUser";
		const string password = "TestPassword";
		_signInManager.SignIn(username, password).Returns((SignInResult.Success, null, false));

		var result = await _jwtAuthenticator.Authenticate(username, password);

		Assert.AreEqual("", result);
	}

	[TestMethod]
	public async Task Authenticate_UserWithNoClaims_ReturnsValidToken()
	{
		const string username = "TestUser";
		const string password = "TestPassword";
		var user = new User
		{
			Id = 456,
			UserName = username,
			Email = "noclaims@example.com"
		};

		_userManager.GetClaims(user).Returns([]);
		_signInManager.SignIn(username, password).Returns((SignInResult.Success, user, false));

		var result = await _jwtAuthenticator.Authenticate(username, password);

		Assert.IsNotNull(result);
		Assert.IsTrue(result.Length > 0);

		var tokenHandler = new JsonWebTokenHandler();
		var key = Encoding.ASCII.GetBytes(_appSettings.Jwt.SecretKey);
		var validationParameters = new TokenValidationParameters
		{
			IssuerSigningKey = new SymmetricSecurityKey(key),
			ValidateIssuer = false,
			ValidateAudience = false,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};

		var validationResult = await tokenHandler.ValidateTokenAsync(result, validationParameters);
		Assert.IsTrue(validationResult.IsValid);
	}

	[TestMethod]
	public async Task Authenticate_ValidCredentials_TokenHasCorrectExpiration()
	{
		const string username = "TestUser";
		const string password = "TestPassword";
		var user = new User
		{
			Id = 789,
			UserName = username,
			Email = "expiry@example.com"
		};

		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id.ToString())
		};

		_userManager.GetClaims(user).Returns(claims);
		_signInManager.SignIn(username, password)
			.Returns((SignInResult.Success, user, false));

		var beforeAuth = DateTime.UtcNow;

		var result = await _jwtAuthenticator.Authenticate(username, password);

		var afterAuth = DateTime.UtcNow;
		var jsonWebToken = new JsonWebToken(result);
		var expiry = jsonWebToken.ValidTo;

		// The token should expire approximately 60 minutes from now (within a 5-minute window for test execution time)
		var expectedExpiry = beforeAuth.AddMinutes(_appSettings.Jwt.ExpiresInMinutes);
		var timeDifference = Math.Abs((expiry - expectedExpiry).TotalMinutes);
		Assert.IsTrue(timeDifference < 5, $"Token expiry time difference was {timeDifference} minutes, expected less than 5");
		Assert.IsTrue(expiry > afterAuth, "Token should expire in the future");
	}

	[TestMethod]
	public async Task Authenticate_MultipleCalls_GeneratesDifferentTokens()
	{
		const string username = "TestUser";
		const string password = "TestPassword";
		var user = new User
		{
			Id = 999,
			UserName = username,
			Email = "multi@example.com"
		};

		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id.ToString())
		};

		_userManager.GetClaims(user).Returns(claims);
		_signInManager.SignIn(username, password).Returns((SignInResult.Success, user, false));

		var token1 = await _jwtAuthenticator.Authenticate(username, password);
		await Task.Delay(1100); // Delay to ensure different timestamps
		var token2 = await _jwtAuthenticator.Authenticate(username, password);

		Assert.IsNotNull(token1);
		Assert.IsNotNull(token2);
		Assert.AreNotEqual(token1, token2, "Each authentication should generate a unique token");
	}

	[TestMethod]
	public async Task Authenticate_ValidCredentials_TokenUsesHmacSha256Algorithm()
	{
		const string username = "TestUser";
		const string password = "TestPassword";
		var user = new User
		{
			Id = 111,
			UserName = username,
			Email = "algorithm@example.com"
		};

		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id.ToString())
		};

		_userManager.GetClaims(user).Returns(claims);
		_signInManager.SignIn(username, password).Returns((SignInResult.Success, user, false));

		var result = await _jwtAuthenticator.Authenticate(username, password);

		var jsonWebToken = new JsonWebToken(result);
		Assert.IsTrue(
			jsonWebToken.Alg is "HS256" or "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
			$"Token should use HMAC SHA256 algorithm, but got: {jsonWebToken.Alg}");
	}
}
