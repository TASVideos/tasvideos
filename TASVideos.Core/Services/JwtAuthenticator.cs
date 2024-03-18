﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services;

public interface IJwtAuthenticator
{
	Task<string> Authenticate(string username, string password);
}

internal class JwtAuthenticator(
	ApplicationDbContext db,
	UserManager userManager,
	SignInManager signInManager,
	AppSettings settings)
	: IJwtAuthenticator
{
	private readonly AppSettings.JwtSettings _settings = settings.Jwt;

	public async Task<string> Authenticate(string userName, string password)
	{
		var result = await signInManager.SignIn(userName, password);
		if (!result.Succeeded)
		{
			return "";
		}

		var user = await db.Users.SingleOrDefaultAsync(u => u.UserName == userName);
		if (user is null)
		{
			return "";
		}

		var claims = await userManager.GetClaimsAsync(user);
		var tokenHandler = new JwtSecurityTokenHandler();
		var tokenKey = Encoding.ASCII.GetBytes(_settings.SecretKey);
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes),
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(tokenKey),
				SecurityAlgorithms.HmacSha256Signature)
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		var jwtToken = tokenHandler.WriteToken(token);
		return jwtToken;
	}
}
