#pragma warning disable 1591
namespace TASVideos.Api.Settings
{
	public class JwtSettings
	{
		public string SecretKey { get; set; } = "";
		public int ExpiresInMinutes { get; set; }
	}
}
