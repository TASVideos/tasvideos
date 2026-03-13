using TASVideos.Data.Entity.Forum;

namespace TASVideos.Api.Endpoints;

internal static class ForumTopicEndpoints
{
	extension(WebApplication app)
	{
		public WebApplication MapForumTopics()
		{
			var group = app.MapApiGroup("ForumTopics");

			group.MapGet("{id:int}", async (int id, HttpContext context, ApplicationDbContext db) =>
			{
				var canSeeRestricted = context.User.Has(PermissionTo.SeeRestrictedForums);
				var topics = await db.ForumTopics
					.ExcludeRestricted(canSeeRestricted)
					.ToForumTopicResponse()
					.SingleOrDefaultAsync(t => t.Id == id);

				return ApiResults.OkOr404(topics);
			});

			group.MapGet("{id:int}/posts", async (int id, [AsParameters] ApiRequest request, HttpContext context, ApplicationDbContext db) =>
			{
				var canSeeRestricted = context.User.Has(PermissionTo.SeeRestrictedForums);
				var posts = (await db.ForumPosts
					.ExcludeRestricted(canSeeRestricted)
					.ForTopic(id)
					.ToForumPostResponse()
					.SortAndPaginate(request)
					.ToListAsync())
					.FieldSelect(request);

				return Results.Ok(posts);
			});

			group.MapGet("{id:int}/poll", async (int id, HttpContext context, ApplicationDbContext db) =>
			{
				var canSeeRestricted = context.User.Has(PermissionTo.SeeRestrictedForums);

				var topicExists = await db.ForumTopics
					.ExcludeRestricted(canSeeRestricted)
					.AnyAsync(f => f.Id == id);
				if (!topicExists)
				{
					return Results.NotFound();
				}

				var poll = await db.ForumPolls
					.Where(p => p.TopicId == id)
					.Select(p => new
					{
						p.Id,
						p.TopicId,
						p.Question,
						p.CloseDate,
						p.MultiSelect,
						p.CreateTimestamp,
						p.LastUpdateTimestamp
					})
					.SingleOrDefaultAsync();

				return ApiResults.OkOr404(poll);
			});

			return app;
		}

		public WebApplication MapSubforums()
		{
			var group = app.MapApiGroup("SubForums");

			group.MapGet("{id:int}", async (int id, HttpContext context, ApplicationDbContext db) =>
			{
				var canSeeRestricted = context.User.Has(PermissionTo.SeeRestrictedForums);
				var topics = await db.Forums
					.ExcludeRestricted(canSeeRestricted)
					.Select(f => new
					{
						f.Id,
						f.CategoryId,
						f.Name,
						f.ShortName,
						f.Description,
						f.Ordinal,
						f.Restricted,
						f.CanCreateTopics,
						f.CreateTimestamp,
						f.LastUpdateTimestamp
					})
					.SingleOrDefaultAsync(t => t.Id == id);

				return ApiResults.OkOr404(topics);
			});

			group.MapGet("{id:int}/topics", async (int id, [AsParameters] ApiRequest request, HttpContext context, ApplicationDbContext db) =>
			{
				var canSeeRestricted = context.User.Has(PermissionTo.SeeRestrictedForums);
				var topics = (await db.ForumTopics
					.ForForum(id)
					.ExcludeRestricted(canSeeRestricted)
					.ToForumTopicResponse()
					.SortAndPaginate(request)
					.ToListAsync())
					.FieldSelect(request);

				return Results.Ok(topics);
			});

			return app;
		}
	}
}
