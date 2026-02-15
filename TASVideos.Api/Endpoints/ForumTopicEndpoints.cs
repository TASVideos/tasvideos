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
					.Select(t => new
					{
						t.Id,
						t.ForumId,
						t.Title,
						t.PosterId,
						Type = t.Type.ToString(),
						t.IsLocked,
						t.PollId,
						t.SubmissionId,
						t.GameId,
						t.CreateTimestamp,
						t.LastUpdateTimestamp
					})
					.SingleOrDefaultAsync(t => t.Id == id);

				return ApiResults.OkOr404(topics);
			});

			group.MapGet("{id:int}/posts", async (int id, HttpContext context, ApplicationDbContext db) =>
			{
				var canSeeRestricted = context.User.Has(PermissionTo.SeeRestrictedForums);
				var posts = await db.ForumPosts
					.ExcludeRestricted(canSeeRestricted)
					.ForTopic(id)
					.Select(p => new
					{
						p.Id,
						p.TopicId,
						p.ForumId,
						p.PosterId,
						p.Subject,
						p.Text,
						p.PostEditedTimestamp,
						p.EnableBbCode,
						p.EnableHtml,
						p.PosterMood,
						p.CreateTimestamp,
						p.LastUpdateTimestamp
					})
					.ToListAsync();

				return Results.Ok(posts);
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

			return app;
		}
	}
}
