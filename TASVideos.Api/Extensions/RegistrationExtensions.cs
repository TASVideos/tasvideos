using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

namespace TASVideos.Api;

internal static class RegistrationExtensions
{
	public static RouteGroupBuilder MapApiGroup(this WebApplication app, string group)
	{
		return app.MapGroup($"api/v1/{group.ToLower()}").WithTags(group);
	}

	public static RouteHandlerBuilder ProducesFromId<T>(this RouteHandlerBuilder builder, string resource)
	{
		return builder
			.Produces<T>()
			.WithSummary($"Returns a {resource} with the given id.")
			.WithOpenApi(g =>
			{
				g.Responses.AddGeneric400();
				g.Responses.Add404ById(resource);
				return g;
			});
	}

	public static RouteHandlerBuilder Receives<T>(this RouteHandlerBuilder builder)
	{
		return builder.WithOpenApi(g =>
		{
			g.Parameters.Describe<T>();
			g.Responses.AddGeneric400();
			return g;
		});
	}

	public static RouteHandlerBuilder ProducesList<T>(this RouteHandlerBuilder builder, string summary)
	{
		return builder.WithSummary($"Returns {summary}").Produces<IEnumerable<T>>().WithOpenApi();
	}

	public static void Add(this OpenApiResponses responses, int statusCode, string description)
	{
		responses.Add(statusCode.ToString(), new OpenApiResponse { Description = description });
	}

	public static void AddGeneric400(this OpenApiResponses responses)
	{
		responses.Add("400", new OpenApiResponse { Description = "The request parameters are invalid." });
	}

	public static void Add404ById(this OpenApiResponses responses, string resourceName)
	{
		responses.Add("404", new OpenApiResponse { Description = $"{resourceName} with the given id could not be found" });
	}

	// SwaggerParameter from Swashbuckle.AspNetCore.Annotations should be able to do this automatically but there is an outstanding bug, so we need to do this ourselves
	private static void Describe<T>(this IList<OpenApiParameter> list)
	{
		foreach (var prop in typeof(T).GetProperties())
		{
			var swaggerParameter = prop.GetCustomAttribute<SwaggerParameterAttribute>();
			if (swaggerParameter is null)
			{
				continue;
			}

			var parameter = list.FirstOrDefault(p => p.Name == prop.Name);
			if (parameter is not null)
			{
				parameter.Description = swaggerParameter.Description;
			}
		}
	}
}
