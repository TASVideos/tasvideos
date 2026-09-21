using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Reactions;

public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public ReactionsPagingRequest Search { get; set; } = new();

	public PageOf<ReactionEntry, ReactionsPagingRequest> Reactions { get; set; } = new([], new());

	public async Task<IActionResult> OnGet()
	{
		var userCanSeeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		if (Search.PostId is not null)
		{
			var postExists = await db.ForumPosts
				.ExcludeRestricted(userCanSeeRestricted)
				.AnyAsync(p => p.Id == Search.PostId);

			if (!postExists)
			{
				return NotFound();
			}
		}

		if (Search.UserId is not null)
		{
			var userExists = await db.Users.AnyAsync(u => u.Id == Search.UserId);
			if (!userExists)
			{
				return NotFound();
			}
		}

		var query = db.ForumPostReactions
			.Where(r => userCanSeeRestricted || !r.Post!.Topic!.Forum!.Restricted)
			.Select(r => new ReactionEntry
			{
				Id = r.Id,
				Reaction = r.Reaction,
				UserId = r.UserId,
				User = r.User!.UserName,
				Post = r.PostId,
				Date = r.LastUpdateTimestamp
			});

		if (Search.PostId is not null)
		{
			query = query.Where(r => r.Post == Search.PostId);
		}

		if (Search.UserId is not null)
		{
			query = query.Where(r => r.UserId == Search.UserId);
		}

		Reactions = await query
			.SortedPageOf(Search);

		return Page();
	}

	public async Task<IActionResult> OnPostDelete(int? reactionId)
	{
		if (!User.Has(PermissionTo.ModerateReactions))
		{
			return AccessDenied();
		}

		if (reactionId is null)
		{
			return NotFound();
		}

		var userCanSeeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var reaction = await db.ForumPostReactions
			.Where(r => userCanSeeRestricted || !r.Post!.Topic!.Forum!.Restricted)
			.FirstOrDefaultAsync(r => r.Id == reactionId);

		if (reaction is null)
		{
			return NotFound();
		}

		db.ForumPostReactions.Remove(reaction);
		await db.SaveChangesAsync();

		return BaseReturnUrlRedirect();
	}

	public class ReactionEntry
	{
		[TableIgnore]
		public int Id { get; set; }
		[Sortable]
		public string Reaction { get; set; } = "";
		[TableIgnore]
		public int UserId { get; set; }
		[Sortable]
		public string User { get; set; } = "";
		[Sortable]
		public DateTime Date { get; set; }
		[Sortable]
		public int Post { get; set; }
	}

	[PagingDefaults(PageSize = 50, Sort = $"-{nameof(ReactionEntry.Date)}")]
	public class ReactionsPagingRequest : PagingModel
	{
		public int? PostId { get; set; }
		public int? UserId { get; set; }
	}
}
