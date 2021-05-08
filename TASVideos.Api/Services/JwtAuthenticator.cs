using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TASVideos.Core.Services;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Data.Entity;

#pragma warning disable 1591
namespace TASVideos.Api.Services
{
	public interface IJwtAuthenticator
	{
		Task<string> Authenticate(string username, string password);
	}

	public class JwtAuthenticator : IJwtAuthenticator
	{
		private readonly AppSettings.JwtSettings _settings;
		private readonly ApplicationDbContext _db;
		private readonly UserManager _userManager;
		private readonly SignInManager<User> _signInManager;

		public JwtAuthenticator(
			ApplicationDbContext db,
			UserManager userManager,
			SignInManager<User> signInManager,
			AppSettings.JwtSettings settings)
		{
			_db = db;
			_userManager = userManager;
			_signInManager = signInManager;
			_settings = settings;
		}

		public async Task<string> Authenticate(string username, string password)
		{
			var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == username);
			if (user == null)
			{
				return "";
			}

			var result = await _signInManager.PasswordSignInAsync(user, password, false, true);
			if (!result.Succeeded)
			{
				return "";
			}

			await _userManager.AddUserPermissionsToClaims(user);

			var claims = await _userManager.GetClaimsAsync(user);
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
}
