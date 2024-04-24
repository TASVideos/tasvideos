using Microsoft.OpenApi.Models;

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
		.Produces<string>()
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add("401", new OpenApiResponse { Description = "The sign in failed." });
			return g;
		});

		return app;
	}
}
