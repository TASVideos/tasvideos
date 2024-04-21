using FluentValidation.Results;

namespace TASVideos.Api;

internal static class ApiResults
{
	public static IResult OkOr404(object? result)
	{
		return result is null
			? NotFound()
			: Results.Ok(result);
	}

	public static IResult ValidationError(ValidationResult validationResult)
	{
		return Results.ValidationProblem(validationResult.ToDictionary());
	}

	public static IResult Unauthorized()
	{
		return Results.Json(new { Title = "Unauthorized", Status = 401 }, statusCode: 401);
	}

	public static IResult Forbid()
	{
		return Results.Json(new { Title = "Forbidden", Status = 403 }, statusCode: 403);
	}

	public static IResult NotFound()
	{
		return Results.Json(new { Title = "Not Found", Status = 404 }, statusCode: 404);
	}
}
