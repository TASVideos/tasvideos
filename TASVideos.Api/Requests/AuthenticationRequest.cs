using System.ComponentModel.DataAnnotations;

namespace TASVideos.Api.Requests
{
	/// <summary>
	/// Represents the username and password for an authentication request
	/// </summary>
	public class AuthenticationRequest
	{
		/// <summary>
		/// Gets or sets the username of the user to sign in as
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string Username { get; set; } = "";

		/// <summary>
		/// Gets or sets the password of the user to sign in as
		/// </summary>
		[Required(AllowEmptyStrings = false)]
		public string Password { get; set; } = "";
	}
}
