﻿using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public class SignInManager : SignInManager<User>
{
	private readonly ApplicationDbContext _db;
	private readonly UserManager _userManager;

	public SignInManager(
		ApplicationDbContext db,
		UserManager userManager,
		IHttpContextAccessor contextAccessor,
		IUserClaimsPrincipalFactory<User> claimsFactory,
		IOptions<IdentityOptions> optionsAccessor,
		ILogger<SignInManager<User>> logger,
		IAuthenticationSchemeProvider schemes,
		IUserConfirmation<User> confirmation)
		: base(
			userManager,
			contextAccessor,
			claimsFactory,
			optionsAccessor,
			logger,
			schemes,
			confirmation)
	{
		_db = db;
		_userManager = userManager;
	}

	public async Task<SignInResult> SignIn(string userName, string password, bool rememberMe = false)
	{
		var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == userName);
		if (user is null)
		{
			return SignInResult.Failed;
		}

		var claims = await _userManager.AddUserPermissionsToClaims(user);
		var canLogIn = claims.Permissions().Contains(PermissionTo.Login);

		if (!canLogIn)
		{
			return SignInResult.NotAllowed;
		}

		var result = await base.PasswordSignInAsync(
			userName,
			password,
			rememberMe,
			lockoutOnFailure: true);

		if (result.Succeeded)
		{
			user.LastLoggedInTimeStamp = DateTime.UtcNow;

			// Note: This runs a save changes so LastLoggedInTimeStamp will get updated too
			await _userManager.AddUserPermissionsToClaims(user);
		}

		return result;
	}

	public async Task<IdentityResult> AddPassword(ClaimsPrincipal principal, string newPassword)
	{
		var user = await UserManager.GetUserAsync(principal);
		var result = await UserManager.AddPasswordAsync(user, newPassword);

		if (result.Succeeded)
		{
			await SignInAsync(user, isPersistent: false);
		}

		return result;
	}

	public async Task<bool> UsernameIsAllowed(string userName)
	{
		var disallows = await _db.UserDisallows.ToListAsync();
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
		return await _db.Users.AnyAsync(u => EF.Functions.Like(u.Email, baseEmail));
	}

	public async Task<bool> EmailAndUserNameMatch(string username, string email)
	{
		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username))
		{
			return false;
		}

		return await _db.Users.AnyAsync(u => u.Email == email && u.UserName == username);
	}
}
