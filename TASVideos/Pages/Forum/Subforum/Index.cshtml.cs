using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Subforum.Models;

namespace TASVideos.Pages.Forum.Subforum
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromQuery]
		public ForumRequest Search { get; set; }

		[FromRoute]
		public int Id { get; set; }

		public ForumDisplayModel Forum { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Forum = await _db.Forums
				.ExcludeRestricted(User.Has(PermissionTo.SeeRestrictedForums))
				.Select(f => new ForumDisplayModel
				{
					Id = f.Id,
					Name = f.Name,
					Description = f.Description
				})
				.SingleOrDefaultAsync(f => f.Id == Id);

			if (Forum == null)
			{
				return NotFound();
			}

			var rowsToSkip = Search.Offset();
			var rowCount = await _db.ForumTopics
				.ForForum(Id)
				.CountAsync();

			var results = await _db.ForumTopics
				.ForForum(Id)
				.Select(ft => new ForumDisplayModel.ForumTopicEntry
				{
					Id = ft.Id,
					Title = ft.Title,
					CreateUserName = ft.CreateUserName,
					CreateTimestamp = ft.CreateTimeStamp,
					Type = ft.Type,
					Views = ft.Views,
					PostCount = ft.ForumPosts.Count,
					LastPost = ft.ForumPosts.Max(fp => (DateTime?)fp.CreateTimeStamp)
				})
				.OrderByDescending(ft => ft.Type == ForumTopicType.Sticky)
				.ThenByDescending(ft => ft.Type == ForumTopicType.Announcement)
				.ThenByDescending(ft => ft.LastPost)
				.Skip(rowsToSkip)
				.Take(Search.PageSize ?? ForumConstants.TopicsPerForum)
				.ToListAsync();

			Forum.Topics = new PageOf<ForumDisplayModel.ForumTopicEntry>(results)
			{
				PageSize = Search.PageSize,
				CurrentPage = Search.CurrentPage,
				RowCount = rowCount,
				Sort = Search.Sort
			};

			Forum.Description = RenderHtml(Forum.Description);
			return Page();
		}
	}
}
