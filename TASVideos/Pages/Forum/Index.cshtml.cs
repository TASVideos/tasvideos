using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Forum.Models;

namespace TASVideos.Pages.Forum
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public ICollection<ForumCategoryModel> Categories { get; set; } = new List<ForumCategoryModel>();

		public async Task OnGet()
		{
			Categories = await _db.ForumCategories
				.Select(c => new ForumCategoryModel
				{
					Id = c.Id,
					Ordinal = c.Ordinal,
					Title = c.Title,
					Description = c.Description,
					Forums = c.Forums
						.Select(f => new ForumCategoryModel.Forum
						{
							Id = f.Id,
							Ordinal = f.Ordinal,
							Restricted = f.Restricted,
							Name = f.Name,
							Description = f.Description,
							LastPost = f.ForumPosts
								.Select(fp => new ForumCategoryModel.Forum.Post
								{
									Id = fp.Id,
									CreateTimestamp = fp.CreateTimestamp,
									CreateUserName = fp.CreateUserName
								})
								.SingleOrDefault(fp => fp.Id == f.ForumPosts.Max(fpp => fpp.Id))
						})
				})
				.ToListAsync();
		}
	}
}
