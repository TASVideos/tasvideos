using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Api.Controllers;

internal static class TagsApiMapper
{
	public static void Map(WebApplication app)
	{
		app.MapGet("api/v1/tags/{id}", async (int id, ITagService tagService) =>
		{
			var tag = await tagService.GetById(id);
			return tag is null ? Results.NotFound() : Results.Ok(tag);
		})
		.WithTags("Tags")
		.WithSummary("Returns a tag with the given id.")
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add404ById("tag");
			return g;
		})
		.WithName("GetByTagId");

		app.MapGet("api/v1/tags", async (ApiRequest request, IValidator<ApiRequest> validator, ITagService tagService) =>
		{
			var validationResult = validator.Validate(request);
			if (!validationResult.IsValid)
			{
				return Results.ValidationProblem(validationResult.ToDictionary());
			}

			var tags = (await tagService.GetAll())
				.AsQueryable()
				.SortBy(request)
				.Paginate(request)
				.AsEnumerable()
				.FieldSelect(request);

			return Results.Ok(tags);
		})
		.WithTags("Tags")
		.WithSummary("Returns a list of available tags")
		.WithOpenApi(g =>
		{
			g.Parameters.AddBaseQueryParams();
			g.Responses.AddGeneric400();
			return g;
		});

		app.MapPost("api/v1/tags", async (TagAddEditRequest request, ITagService tagService) =>
		{
			var (id, result) = await tagService.Add(request.Code, request.DisplayName);

			switch (result)
			{
				case TagEditResult.DuplicateCode:
					var error = new Dictionary<string, string>
					{
						["Code"] = $"{request.Code} already exists"
					};
					return Results.Conflict(error);
				case TagEditResult.Success:
					return Results.CreatedAtRoute(routeName: "GetByTagId", routeValues: new { id });
				default:
					return Results.BadRequest();
			}
		})
		.WithTags("Tags")
		.WithSummary("Creates a new tag");
	}
}
