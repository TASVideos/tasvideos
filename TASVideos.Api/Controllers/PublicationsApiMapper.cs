﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace TASVideos.Api.Controllers;

// TODO: document types (integer($int32), string, etc
// Model validation
// JWT authentication
public static class PublicationsApiMapper
{
	public static void MapEndpoints(WebApplication app)
	{
		app.MapGet("api/v1/publications/{id}", async (int id, ApplicationDbContext db) =>
		{
			var pub = await db.Publications
				.ToPublicationsResponse()
				.SingleOrDefaultAsync(p => p.Id == id);

			return pub is null
				? Results.NotFound()
				: Results.Ok(pub);
		})
		.WithTags("Publications")
		.WithSummary("Returns a publication with the given id.")
		.Produces<PublicationsResponse>()
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add404ById("publication");
			return g;
		});

		app.MapGet("api/v1/publications", async (PublicationsRequest request, ApplicationDbContext db) =>
		{
			var pubs = (await db.Publications
					.FilterByTokens(request)
					.ToPublicationsResponse()
					.SortBy(request)
					.Paginate(request)
					.ToListAsync())
				.FieldSelect(request);
			return Results.Ok(pubs);
		})
		.WithTags("Publications")
		.WithSummary("Returns a list of publications, filtered by the given criteria.")
		.Produces<IEnumerable<PublicationsResponse>>()
		.WithOpenApi(g =>
		{
			g.Parameters.AddFromQuery("systems", "The system codes to filter by");
			g.Parameters.AddFromQuery("classNames", "The publication class names to filter by");
			g.Parameters.AddFromQuery("startYear", "The start year to filter by");
			g.Parameters.AddFromQuery("endYear", "The end year to filter by");
			g.Parameters.AddFromQuery("genreNames", "the genres to filter by");
			g.Parameters.AddFromQuery("tagNames", "the names of the publication tags to filter by");
			g.Parameters.AddFromQuery("flagNames", "the names of the publication flags to filter by");
			g.Parameters.AddFromQuery("authorIds", "the ids of the authors to filter by");
			g.Parameters.AddFromQuery("showObsoleted", "indicates whether or not to return obsoleted publications");
			g.Parameters.AddFromQuery("onlyObsoleted", "indicates whether or not to only return obsoleted publications");
			g.Parameters.AddFromQuery("gameIds", "the ids of the games to filter by");
			g.Parameters.AddFromQuery("gameGroupIds", "the ids of the game groups to filter by");
			g.Parameters.AddBaseQueryParams();
			g.Responses.AddGeneric400();
			return g;
		});
	}
}

// TODO: move me
public static class Extensions
{
	public static void AddGeneric400(this OpenApiResponses responses)
	{
		responses.Add("400", new OpenApiResponse { Description = "The request parameters are invalid." });
	}

	public static void Add404ById(this OpenApiResponses responses, string resourceName)
	{
		responses.Add("404", new OpenApiResponse { Description = $"Could not find {resourceName} with the given id" });
	}

	public static void AddFromQuery(this IList<OpenApiParameter> list, string name, string description)
	{
		list.Add(new OpenApiParameter { In = ParameterLocation.Query, Name = name, Description = description });
	}

	public static void AddBaseQueryParams(this IList<OpenApiParameter> list)
	{
		list.AddFromQuery(
			"pageSize",
			"""
			The total number of records to return.
			If not specified, then a default number of records will be returned
			""");
		list.AddFromQuery(
			"currentPage",
			"""
			The page to start returning records.
			If not specified, then an offset of 1 will be used
			""");
		list.AddFromQuery(
			"sort",
			"""
			The fields to sort by.
			If multiple sort parameters, the list should be comma separated.
			Precede the parameter with a + or - to sort ascending or descending respectively.
			If not specified, then a default sort will be used
			""");
		list.AddFromQuery(
			"fields",
			"""
			The fields to return.
			If multiple, fields must be comma separated.
			If not specified, then all fields will be returned
			""");
	}
}