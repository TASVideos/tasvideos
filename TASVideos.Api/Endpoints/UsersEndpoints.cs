using Microsoft.OpenApi;

namespace TASVideos.Api;

internal static class UsersEndpoints
{
	public static WebApplication MapUsers(this WebApplication app)
	{
		app.MapPost("api/v1/users/authenticate", async (AuthenticationRequest request, HttpContext context, IJwtAuthenticator jwtAuthenticator) =>
		{
			var validationError = ApiResults.Validate(request, context);
			if (validationError is not null)
			{
				return validationError;
			}

			var token = await jwtAuthenticator.Authenticate(request.Username, request.Password);
			return string.IsNullOrWhiteSpace(token)
				? ApiResults.Unauthorized()
				: Results.Ok(token);
		})
		.WithTags("Users")
		.WithSummary("Signs in a user and returns a JWT token.")
		.WithDescription("""
						<p>200 if the sign in was successful</p>
						<p>401 if the sign in failed</p>
						""")
		.Produces<string>()
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status401Unauthorized);

		return app;
	}
}
