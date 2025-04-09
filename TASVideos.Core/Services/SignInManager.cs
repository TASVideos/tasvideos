using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TASVideos.Core.Services;

public interface ISignInManager
{
	Task<(SignInResult Result, User? User, bool FailedDueToBan)> SignIn(string userName, string password, bool rememberMe = false);
	Task<bool> UsernameIsAllowed(string userName);
	Task<bool> EmailExists(string email);
	bool IsPasswordAllowed(string? userName, string? email, string? password);
	Task SignIn(User user, bool isPersistent, string? authenticationMethod = null);
	Task Logout(ClaimsPrincipal user);
	Task<bool> HasPassword(ClaimsPrincipal user);
	Task<IdentityResult> AddPassword(ClaimsPrincipal principal, string newPassword);
	bool IsSignedIn(ClaimsPrincipal principal);
}

internal class SignInManager(
	ApplicationDbContext db,
	UserManager userManager,
	IHttpContextAccessor contextAccessor,
	IUserClaimsPrincipalFactory<User> claimsFactory,
	IOptions<IdentityOptions> optionsAccessor,
	ILogger<SignInManager<User>> logger,
	IAuthenticationSchemeProvider schemes,
	IUserConfirmation<User> confirmation)
	: SignInManager<User>(userManager,
		contextAccessor,
		claimsFactory,
		optionsAccessor,
		logger,
		schemes,
		confirmation), ISignInManager
{
	public new UserManager UserManager => (UserManager)base.UserManager;

	public Task SignIn(User user, bool isPersistent, string? authenticationMethod = null)
		=> SignInAsync(user, isPersistent, authenticationMethod);

	public async Task<(SignInResult Result, User? User, bool FailedDueToBan)> SignIn(string userName, string password, bool rememberMe = false)
	{
		userName = userName.Trim().Replace(" ", "_");
		var user = await db.Users.ForUser(userName).SingleOrDefaultAsync();
		if (user is null)
		{
			return (SignInResult.Failed, null, FailedDueToBan: false);
		}

		var result = await CheckPasswordSignInAsync(user, password, true);

		if (result.Succeeded)
		{
			if (user.IsBanned())
			{
				return (SignInResult.Failed, user, FailedDueToBan: true);
			}

			user.LastLoggedInTimeStamp = DateTime.UtcNow;

			// Note: This runs a save changes so LastLoggedInTimeStamp will get updated too
			await userManager.AddUserPermissionsToClaims(user);
			await SignInAsync(user, rememberMe);
		}

		return (result, user, FailedDueToBan: false);
	}

	public async Task<IdentityResult> AddPassword(ClaimsPrincipal principal, string newPassword)
	{
		var user = await userManager.GetRequiredUser(principal);
		var result = await UserManager.AddPasswordAsync(user, newPassword);

		if (result.Succeeded)
		{
			await SignInAsync(user, isPersistent: false);
		}

		return result;
	}

	public async Task<bool> UsernameIsAllowed(string userName)
	{
		var disallows = await db.UserDisallows.ToListAsync();
		foreach (var disallow in disallows)
		{
			var regex = new Regex(disallow.RegexPattern);
			if (regex.IsMatch(userName))
			{
				return false;
			}
		}

		return true;
	}

	public async Task<bool> EmailExists(string email)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			return false;
		}

		var baseEmail = email.Split('+')[0]; // Strip off alias
		return await db.Users.AnyAsync(u => EF.Functions.Like(u.Email, baseEmail));
	}

	public async Task Logout(ClaimsPrincipal user)
	{
		var u = await userManager.GetRequiredUser(user);
		await userManager.RemoveClaimsAsync(u, user.Claims);
		await SignOutAsync();
	}

	/// <summary>
	/// Attempts to prevent some really basic bad behaviors such as same username as password, because people do these things
	/// Note that this does not enforce any password requirements, only compares it to username, and email
	/// </summary>
	public bool IsPasswordAllowed(string? userName, string? email, string? password)
	{
		if (userName == password)
		{
			return false;
		}

		if (email == password)
		{
			return false;
		}

		if (email?.Split('@').First() == password)
		{
			return false;
		}

		return true;
	}

	public async Task<bool> HasPassword(ClaimsPrincipal user)
	{
		var u = await UserManager.GetRequiredUser(user);
		return await UserManager.HasPasswordAsync(u);
	}
}
