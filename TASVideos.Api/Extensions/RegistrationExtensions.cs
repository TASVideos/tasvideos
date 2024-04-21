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

	public static void AddFromQuery(this IList<OpenApiParameter> list, string name, string description, Type type)
	{
		list.Add(new OpenApiParameter { In = ParameterLocation.Query, Name = name, Description = description, Schema = new OpenApiSchema { Type = GenerateType(type) } });
	}

	public static void AddBoolFromQuery(this IList<OpenApiParameter> list, string name, string description)
	{
		list.Add(new OpenApiParameter { In = ParameterLocation.Query, Name = name, Description = description, Schema = new OpenApiSchema { Type = GenerateType(typeof(bool)) } });
	}

	public static void AddIntFromQuery(this IList<OpenApiParameter> list, string name, string description)
	{
		list.Add(new OpenApiParameter { In = ParameterLocation.Query, Name = name, Description = description, Schema = new OpenApiSchema { Type = GenerateType(typeof(int)) } });
	}

	public static void AddStringFromQuery(this IList<OpenApiParameter> list, string name, string description)
	{
		list.Add(new OpenApiParameter { In = ParameterLocation.Query, Name = name, Description = description, Schema = new OpenApiSchema { Type = GenerateType(typeof(string)) } });
	}

	public static void AddBaseQueryParams(this IList<OpenApiParameter> list)
	{
		list.AddFromQuery(
			"pageSize",
			"""
			The total number of records to return.
			If not specified, then a default number of records will be returned
			""",
			typeof(int));
		list.AddFromQuery(
			"currentPage",
			"""
			The page to start returning records.
			If not specified, then an offset of 1 will be used
			""",
			typeof(int));
		list.AddFromQuery(
			"sort",
			"""
			The fields to sort by.
			If multiple sort parameters, the list should be comma separated.
			Precede the parameter with a + or - to sort ascending or descending respectively.
			If not specified, then a default sort will be used
			""",
			typeof(string));
		list.AddFromQuery(
			"fields",
			"""
			The fields to return.
			If multiple, fields must be comma separated.
			If not specified, then all fields will be returned
			""",
			typeof(string));
	}

	private static string GenerateType(Type type)
	{
		if (type == typeof(int))
		{
			return "integer($int32)";
		}

		if (type == typeof(bool))
		{
			return "boolean";
		}

		if (type == typeof(string))
		{
			return "string";
		}

		throw new NotImplementedException($"support for type {type.Name} not supported yet.");
	}
}
