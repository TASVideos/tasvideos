using FluentValidation.Results;

namespace TASVideos.Api;

internal static class ApiResults
{
	public static IResult Unauthorized()
	{
		return Results.Json(new { Title = "Unauthorized", Status = 401 }, statusCode: 401);
	}

	public static IResult Forbid()
	{
		return Results.Json(new { Title = "Forbidden", Status = 403 }, statusCode: 403);
	}

	public static IResult ValidationError(ValidationResult validationResult)
	{
		return Results.ValidationProblem(validationResult.ToDictionary());
	}

}
