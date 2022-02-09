using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TASVideos.Core.Settings;
using TASVideos.Data;

namespace TASVideos.Core.Services;

public interface IJwtAuthenticator
{
	Task<string> Authenticate(string username, string password);
}

internal class JwtAuthenticator : IJwtAuthenticator
{
	private readonly AppSettings.JwtSettings _settings;
	private readonly ApplicationDbContext _db;
	private readonly UserManager _userManager;
	private readonly SignInManager _signInManager;

	public JwtAuthenticator(
		ApplicationDbContext db,
		UserManager userManager,
		SignInManager signInManager,
		AppSettings settings)
	{
		_db = db;
		_userManager = userManager;
		_signInManager = signInManager;
		_settings = settings.Jwt;
	}

	public async Task<string> Authenticate(string userName, string password)
	{
		var result = await _signInManager.SignIn(userName, password);
		if (!result.Succeeded)
		{
			return "";
		}

		var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == userName);
		if (user == null)
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
}
