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

	public static RouteHandlerBuilder DocumentIdGet(this RouteHandlerBuilder builder, string resource, Type? type = null)
	{
		if (type is not null)
		{
			builder = builder.Produces(200, type);
		}

		return builder
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

	public static void DescribeBaseQueryParams(this IList<OpenApiParameter> list)
	{
		list.Describe(
			"PageSize",
			"""
			The total number of records to return.
			If not specified, then a default number of records will be returned
			""");
		list.Describe(
			"CurrentPage",
			"""
			The page to start returning records.
			If not specified, then an offset of 1 will be used
			""");
		list.Describe(
			"Sort",
			"""
			The fields to sort by.
			If multiple sort parameters, the list should be comma separated.
			Precede the parameter with a + or - to sort ascending or descending respectively.
			If not specified, then a default sort will be used
			""");
		list.Describe(
			"Fields",
			"""
			The fields to return.
			If multiple, fields must be comma separated.
			If not specified, then all fields will be returned
			""");
	}

	public static void Describe(this IList<OpenApiParameter> list, string name, string description)
	{
		var parameter = list.FirstOrDefault(l => l.Name == name);
		if (parameter is not null)
		{
			parameter.Description = description;
		}
	}
}
