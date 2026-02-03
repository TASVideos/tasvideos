namespace TASVideos.Api;

internal static class TagsEndpoints
{
	public static WebApplication MapTags(this WebApplication app)
	{
		var group = app.MapApiGroup("Tags");

		group
			.MapGet("{id:int}", async (int id, ITagService tagService) => ApiResults.OkOr404(
				await tagService.GetById(id)))
			.ProducesFromId<TagsResponse>("tag")
			.WithName("GetByTagId");

		group.MapGet("", async ([AsParameters] ApiRequest request, HttpContext context, ITagService tagService) =>
		{
			var validationError = ApiResults.Validate(request, context);
			if (validationError is not null)
			{
				return validationError;
			}

			var tags = (await tagService.GetAll())
				.AsQueryable()
				.SortAndPaginate(request)
				.AsEnumerable()
				.FieldSelect(request);

			return Results.Ok(tags);
		})
		.Receives<ApiRequest>()
		.ProducesList<TagsResponse>("a list of publication tags");

		group.MapPost("", async (TagAddEditRequest request, ITagService tagService, HttpContext context) =>
		{
			var authError = ApiResults.Authorize(PermissionTo.TagMaintenance, context);
			if (authError is not null)
			{
				return authError;
			}

			var validationError = ApiResults.Validate(request, context);
			if (validationError is not null)
			{
				return validationError;
			}

			var (id, result) = await tagService.Add(request.Code, request.DisplayName);

			return result switch
			{
				TagEditResult.DuplicateCode => ApiResults.Conflict($"{request.Code} already exists"),
				TagEditResult.Success => Results.CreatedAtRoute(routeName: "GetByTagId", routeValues: new { id }),
				_ => ApiResults.BadRequest()
			};
		})
		.WithSummary("Creates a new tag")
		.WithDescription("""
						<p>201 if the tag was created successfully<p>
						<p>400 if the request parameters are invalid</p>
						<p>409 if a tag with the given code already exists<p>
						""")
		.Produces(StatusCodes.Status201Created)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status409Conflict);

		group.MapPut("{id:int}", async (int id, TagAddEditRequest request, ITagService tagService, HttpContext context) =>
		{
			var authError = ApiResults.Authorize(PermissionTo.TagMaintenance, context);
			if (authError is not null)
			{
				return authError;
			}

			var validationError = ApiResults.Validate(request, context);
			if (validationError is not null)
			{
				return validationError;
			}

			var result = await tagService.Edit(id, request.Code, request.DisplayName);
			return result switch
			{
				TagEditResult.NotFound => ApiResults.NotFound(),
				TagEditResult.DuplicateCode => ApiResults.Conflict($"{request.Code} already exists"),
				TagEditResult.Success => Results.Ok(),
				_ => ApiResults.BadRequest()
			};
		})
		.WithSummary("Updates an existing tag")
		.WithDescription("""
						<p>200 if the tag was updated successfully<p>
						<p>400 if the request parameters are invalid</p>
						<p>404 if a tag with the given id was not found</p>
						<p>409 if a tag with the given code already exists<p>
						""")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status400BadRequest)
		.Produces(StatusCodes.Status404NotFound)
		.Produces(StatusCodes.Status409Conflict);

		group.MapDelete("{id:int}", async (int id, ITagService tagService, HttpContext context) =>
		{
			var authError = ApiResults.Authorize(PermissionTo.TagMaintenance, context);
			if (authError is not null)
			{
				return authError;
			}

			var result = await tagService.Delete(id);
			return result switch
			{
				TagDeleteResult.NotFound => ApiResults.NotFound(),
				TagDeleteResult.InUse => ApiResults.Conflict("The tag is in use and cannot be deleted."),
				TagDeleteResult.Success => Results.Ok(),
				_ => ApiResults.BadRequest()
			};
		})
		.WithSummary("Deletes an existing tag")
		.WithDescription("""
						<p>200 if the tag was deleted successfully<p>
						<p>409 if the tag is in use and cannot be deleted<p>
						""")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status409Conflict);

		return app;
	}
}
