using TASVideos.Data.Entity.Forum;
using static TASVideos.Pages.Forum.Topics.IndexModel.PostEntry;

namespace TASVideos.Pages.Forum.Posts;

[AllowAnonymous]
public class ReactionsModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public List<ReactionEntry> Reactions { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var userCanSeeRestricted = User.Has(PermissionTo.SeeRestrictedForums);

		var post = await db.ForumPosts
			.Include(p => p.Reactions)
			.ThenInclude(r => r.User)
			.ExcludeRestricted(userCanSeeRestricted)
			.FirstOrDefaultAsync(p => p.Id == Id);

		if (post is null)
		{
			return NotFound();
		}

		Reactions = post.Reactions
			.OrderBy(r => r.LastUpdateTimestamp)
			.Select(r => new ReactionEntry(r.User!.UserName, r.Reaction))
			.ToList();

		return Page();
	}

	public async Task<IActionResult> OnPost([FromBody] ReactionRequest? request)
	{
		if (!User.Has(PermissionTo.CreateForumPosts))
		{
			return AccessDenied();
		}

		var userCanSeeRestricted = User.Has(PermissionTo.SeeRestrictedForums);

		var post = await db.ForumPosts
			.Include(p => p.Reactions)
			.ExcludeRestricted(userCanSeeRestricted)
			.Where(p => p.PosterId != SiteGlobalConstants.TASVideoAgentId && p.PosterId != SiteGlobalConstants.TASVideosGrueId)
			.FirstOrDefaultAsync(p => p.Id == Id);

		if (post is null)
		{
			return NotFound();
		}

		var reaction = request?.Reaction;
		if (!string.IsNullOrEmpty(reaction) && !SiteGlobalConstants.AllowedReactions.Contains(reaction))
		{
			return BadRequest("Invalid reaction.");
		}

		var existingReaction = post.Reactions.SingleOrDefault(r => r.UserId == User.GetUserId());

		if (existingReaction is not null)
		{
			if (string.IsNullOrEmpty(reaction))
			{
				db.ForumPostReactions.Remove(existingReaction);
			}
			else
			{
				existingReaction.Reaction = reaction;
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(reaction))
			{
				var newReaction = new ForumPostReaction
				{
					UserId = User.GetUserId(),
					PostId = post.Id,
					Reaction = reaction
				};
				db.ForumPostReactions.Add(newReaction);
			}
		}

		await db.SaveChangesAsync();

		var updatedPost = await db.ForumPosts
			.Include(p => p.Reactions)
			.ThenInclude(r => r.User)
			.ExcludeRestricted(userCanSeeRestricted)
			.FirstAsync(p => p.Id == Id);

		return Partial("/Pages/Forum/Topics/_ReactionBar.cshtml", new ReactionSummary
		{
			PostId = Id,
			Reactions = updatedPost.Reactions.Select(r => new ReactionSummary.Entry()
			{
				UserName = r.User!.UserName,
				Reaction = r.Reaction
			}).ToList(),
		});
	}

	public record ReactionEntry(string UserName, string Reaction);
	public record ReactionRequest(string Reaction);
}
