using System.ComponentModel;
using TASVideos.Core;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Subforum;

[AllowAnonymous]
[RequireCurrentPermissions]
public class IndexModel(ApplicationDbContext db, IForumService forumService) : BasePageModel
{
	[FromQuery]
	public ForumRequest Search { get; set; } = new();

	[FromRoute]
	public int Id { get; set; }

	public ForumDisplayModel Forum { get; set; } = new();
	public Dictionary<int, (string PostsCreated, string PostsEdited)> ActivityTopics { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
		var forum = await db.Forums
			.ExcludeRestricted(seeRestricted)
			.Select(f => new ForumDisplayModel
			{
				Id = f.Id,
				Name = f.Name,
				Description = f.Description
			})
			.SingleOrDefaultAsync(f => f.Id == Id);

		if (forum is null)
		{
			return NotFound();
		}

		Forum = forum;
		Forum.Topics = await db.ForumTopics
			.ForForum(Id)
			.Select(ft => new ForumDisplayModel.ForumTopicEntry
			{
				Id = ft.Id,
				Title = ft.Title,
				CreateUserName = ft.Poster!.UserName,
				CreateTimestamp = ft.CreateTimestamp,
				Type = ft.Type,
				IsLocked = ft.IsLocked,
				PostCount = ft.ForumPosts.Count,
				LastPost = ft.ForumPosts
					.Where(fp => fp.Id == ft.ForumPosts.Max(fpp => fpp.Id))
					.Select(fp => new ForumDisplayModel.LastPostEntry
					{
						Id = fp.Id,
						PosterName = fp.Poster!.UserName,
						CreateTimestamp = fp.CreateTimestamp
					})
					.FirstOrDefault()
			})
			.OrderByDescending(ft => ft.Type)
			.ThenByDescending(ft => ft.LastPost!.Id) // The database does not enforce it, but we can assume a topic will always have at least one post
			.PageOf(Search);

		ActivityTopics = await forumService.GetPostActivityOfSubforum(Id);

		return Page();
	}

	public class ForumRequest : PagingModel
	{
		public ForumRequest()
		{
			PageSize = ForumConstants.TopicsPerForum;
		}
	}

	public class ForumDisplayModel
	{
		public int Id { get; init; }
		public string Name { get; init; } = "";
		public string? Description { get; init; }

		public PageOf<ForumTopicEntry> Topics { get; set; } = PageOf<ForumTopicEntry>.Empty();

		public class ForumTopicEntry
		{
			[TableIgnore]
			public int Id { get; init; }

			[DisplayName("Topics")]
			public string Title { get; init; } = "";

			[MobileHide]
			[DisplayName("Replies")]
			public int PostCount { get; init; }

			[MobileHide]
			[DisplayName("Author")]
			public string? CreateUserName { get; init; }

			[TableIgnore]
			public DateTime CreateTimestamp { get; init; }

			[TableIgnore]
			public ForumTopicType Type { get; init; }

			[TableIgnore]
			public bool IsLocked { get; init; }

			[TableIgnore]
			public LastPostEntry? LastPost { get; init; }

			[DisplayName("Last Post")]
			public string? Dummy { get; init; }
		}

		public class LastPostEntry
		{
			public int Id { get; init; }
			public string? PosterName { get; init; }
			public DateTime CreateTimestamp { get; init; }
		}
	}
}
