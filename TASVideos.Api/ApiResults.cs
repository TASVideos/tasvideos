using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

namespace TASVideos.Api;

internal static class ApiResults
{
	[RequiresUnreferencedCode(nameof(NotFound))]
	public static IResult OkOr404(object? result)
	{
		return result is null
			? NotFound()
			: Results.Ok(result);
	}

	public static IResult? Validate<T>(T request, HttpContext context)
	{
		var validator = context.RequestServices.GetRequiredService<IValidator<T>>();
		var validationResult = validator.Validate(request);
		return validationResult.IsValid
			? null
			: Results.ValidationProblem(validationResult.ToDictionary());
	}

	[RequiresUnreferencedCode(nameof(ApiResults))]
	public static IResult? Authorize(PermissionTo permission, HttpContext context)
	{
		if (!context.User.IsLoggedIn())
		{
			return Unauthorized();
		}

		if (!context.User.Has(permission))
		{
			return Forbid();
		}

		return null;
	}

	[RequiresUnreferencedCode(nameof(Results.Json))]
	public static IResult BadRequest()
	{
		return Results.Json(new { Title = "Bad Request", Status = 400 }, statusCode: 400);
	}

	[RequiresUnreferencedCode(nameof(Results.Json))]
	public static IResult Unauthorized()
	{
		return Results.Json(new { Title = "Unauthorized", Status = 401 }, statusCode: 401);
	}

	[RequiresUnreferencedCode(nameof(Results.Json))]
	public static IResult Forbid()
	{
		return Results.Json(new { Title = "Forbidden", Status = 403 }, statusCode: 403);
	}

	[RequiresUnreferencedCode(nameof(Results.Json))]
	public static IResult NotFound()
	{
		return Results.Json(new { Title = "Not Found", Status = 404 }, statusCode: 404);
	}

	[RequiresUnreferencedCode(nameof(Results.Json))]
	public static IResult Conflict(string message)
	{
		return Results.Json(new { Title = "Conflict", Message = message, Status = 409 }, statusCode: 409);
	}
}
