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
		.WithOpenApi(g =>
		{
			g.Responses.Add(201, "The Tag was created successfully.");
			g.Responses.Add(409, "A Tag with the given code already exists.");
			g.Responses.AddGeneric400();
			return g;
		});

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
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add404ById("tag");
			g.Responses.Add(409, "A Tag with the given code already exists.");
			return g;
		});

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
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add404ById("tag");
			g.Responses.Add(409, "The Tag is in use and cannot be deleted.");
			return g;
		});

		return app;
	}
}
