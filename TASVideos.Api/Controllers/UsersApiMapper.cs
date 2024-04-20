using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace TASVideos.Api.Controllers;

internal static class UsersApiMapper
{
	public static WebApplication MapUsers(this WebApplication app)
	{
		app.MapPost("api/v1/users/authenticate", async (AuthenticationRequest request, IValidator<AuthenticationRequest> validator, IJwtAuthenticator jwtAuthenticator) =>
		{
			var validationResult = validator.Validate(request);
			if (!validationResult.IsValid)
			{
				return Results.ValidationProblem(validationResult.ToDictionary());
			}

			var token = await jwtAuthenticator.Authenticate(request.Username, request.Password);
			return string.IsNullOrWhiteSpace(token)
				? Results.Unauthorized()
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
