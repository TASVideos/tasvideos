using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TASVideos.Api.Settings;
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
		private readonly JwtSettings _settings;
		private readonly ApplicationDbContext _db;
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;

		// TODO: put user manager in a common library that both api and mvc can use, also move sign in logic  in Login.cshtml to a service
		public JwtAuthenticator(
			ApplicationDbContext db,
			UserManager<User> userManager,
			SignInManager<User> signInManager,
			JwtSettings settings)
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

			await AddUserPermissionsToClaims(user);
			var result = await _signInManager.PasswordSignInAsync(user, password, false, true);
			if (!result.Succeeded)
			{
				return "";
			}

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

		// TODO: copy pasta from UserManager
		private async Task AddUserPermissionsToClaims(User user)
		{
			var existingClaims = await _userManager.GetClaimsAsync(user);
			if (existingClaims.Any(c => c.Type == CustomClaimTypes.Permission))
			{
				return;
			}

			var permissions = await GetUserPermissionsById(user.Id);
			await _userManager.AddClaimsAsync(user, permissions
				.Select(p => new Claim(CustomClaimTypes.Permission, ((int)p).ToString())));
		}

		/// <summary>
		/// Returns a list of all permissions of the <seea cref="User"/> with the given id
		/// </summary>
		private async Task<IEnumerable<PermissionTo>> GetUserPermissionsById(int userId)
		{
			return await _db.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role!.RolePermission)
				.Select(rp => rp.PermissionId)
				.Distinct()
				.ToListAsync();
		}
	}
}
