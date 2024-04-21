using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;

namespace TASVideos.Api;

internal static class RegistrationExtensions
{
	public static RouteGroupBuilder MapApiGroup(this WebApplication app, string group)
	{
		return app.MapGroup($"api/v1/{group.ToLower()}").WithTags(group);
	}

	public static RouteHandlerBuilder DocumentIdGet(this RouteHandlerBuilder builder, string resource, Type type)
	{
		return builder
			.Produces(200, type)
			.WithSummary($"Returns a {resource} with the given id.")
			.WithOpenApi(g =>
			{
				g.Responses.AddGeneric400();
				g.Responses.Add404ById(resource);
				return g;
			});
	}

	public static void AddGeneric400(this OpenApiResponses responses)
	{
		responses.Add("400", new OpenApiResponse { Description = "The request parameters are invalid." });
	}

	public static void Add404ById(this OpenApiResponses responses, string resourceName)
	{
		responses.Add("404", new OpenApiResponse { Description = $"{resourceName} with the given id could not be found" });
	}

	// SwaggerParameter from Swashbuckle.AspNetCore.Annotations should be able to do this automatically but there is an outstanding bug so we need to do this ourselves
	public static void Describe<T>(this IList<OpenApiParameter> list)
	{
		var props = typeof(T).GetProperties();
		foreach (var prop in props)
		{
			var descriptionAttr = prop.GetCustomAttribute<DescriptionAttribute>();
			if (descriptionAttr is null)
			{
				continue;
			}

			var parameter = list.FirstOrDefault(p => p.Name == prop.Name);
			if (parameter is not null)
			{
				parameter.Description = descriptionAttr.Description;
			}
		}
	}
}
