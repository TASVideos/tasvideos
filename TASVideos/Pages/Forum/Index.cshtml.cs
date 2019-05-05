using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;

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

		public ICollection<ForumCategory> Categories { get; set; } = new List<ForumCategory>();

		public async Task OnGet()
		{
			Categories = await _db.ForumCategories
				.Include(c => c.Forums)
				.ToListAsync();

			foreach (var m in Categories)
			{
				m.Description = RenderHtml(m.Description ?? "");
				foreach (var f in m.Forums)
				{
					f.Description = RenderHtml(f.Description);
				}
			}
		}
	}
}
