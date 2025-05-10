using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services;

public interface IJwtAuthenticator
{
	Task<string> Authenticate(string username, string password);
}

internal class JwtAuthenticator(SignInManager signInManager, AppSettings settings) : IJwtAuthenticator
{
	private readonly AppSettings.JwtSettings _settings = settings.Jwt;

	public async Task<string> Authenticate(string userName, string password)
	{
		var (result, user, _) = await signInManager.SignIn(userName, password);
		if (!result.Succeeded)
		{
			return "";
		}

		if (user is null)
		{
			return "";
		}

		var claims = await signInManager.UserManager.GetClaimsAsync(user);
		var key = Encoding.ASCII.GetBytes(_settings.SecretKey);

		var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes),
			SigningCredentials = new SigningCredentials(
				new SymmetricSecurityKey(key),
				SecurityAlgorithms.HmacSha256Signature)
		});

		return token;
	}
}
