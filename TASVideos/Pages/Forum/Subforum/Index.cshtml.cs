using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Subforum.Models;

namespace TASVideos.Pages.Forum.Subforum
{
	[AllowAnonymous]
	[RequireCurrentPermissions]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromQuery]
		public ForumRequest Search { get; set; } = new ();

		[FromRoute]
		public int Id { get; set; }

		public ForumDisplayModel Forum { get; set; } = new ();

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

			Forum.Topics = await _db.ForumTopics
				.ForForum(Id)
				.Select(ft => new ForumDisplayModel.ForumTopicEntry
				{
					Id = ft.Id,
					Title = ft.Title,
					CreateUserName = ft.CreateUserName,
					CreateTimestamp = ft.CreateTimestamp,
					Type = ft.Type,
					IsLocked = ft.IsLocked,
					PostCount = ft.ForumPosts.Count,
					LastPost = ft.ForumPosts.SingleOrDefault(fp => fp.Id == ft.ForumPosts.Max(fpp => fpp.Id)),
					LastPostDateTime = ft.ForumPosts.SingleOrDefault(fp => fp.Id == ft.ForumPosts.Max(fpp => fpp.Id))!.CreateTimestamp
				})
				.SortedPageOf(Search);

			return Page();
		}
	}
}
