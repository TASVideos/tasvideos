using TASVideos.Api.Requests;

namespace TASVideos.Api.Controllers;

/// <summary>
/// Users and user actions
/// </summary>
public class UsersController : Controller
{
	private readonly IJwtAuthenticator _jwtAuthenticator;

	/// <summary>
	/// Initializes a new instance of the <see cref="UsersController"/> class.
	/// </summary>
	public UsersController(IJwtAuthenticator jwtAuthenticator)
	{
		_jwtAuthenticator = jwtAuthenticator;
	}

	/// <summary>
	/// Signs in a user and returns a JWT token.
	/// </summary>
	/// <response code="200">Returns the JWT token.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="401">The sign in failed.</response>
	[HttpPost("authenticate")]
	public async Task<IActionResult> Authenticate([FromBody] AuthenticationRequest request)
	{
		var token = await _jwtAuthenticator.Authenticate(request.Username, request.Password);
		if (string.IsNullOrWhiteSpace(token))
		{
			return Unauthorized();
		}

		return Ok(token);
	}
}
