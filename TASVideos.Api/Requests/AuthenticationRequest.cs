using System.ComponentModel.DataAnnotations;

namespace TASVideos.Api.Requests;

/// <summary>
/// Represents the username and password for an authentication request
/// </summary>
public class AuthenticationRequest
{
	/// <summary>
	/// Gets the username of the user to sign in as
	/// </summary>
	[Required(AllowEmptyStrings = false)]
	public string Username { get; init; } = "";

	/// <summary>
	/// Gets the password of the user to sign in as
	/// </summary>
	[Required(AllowEmptyStrings = false)]
	public string Password { get; init; } = "";
}
