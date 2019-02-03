using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class PageHistoryModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public PageHistoryModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromQuery]
		public string Path { get; set; }

		public WikiHistoryModel History { get; set; }

		public async Task OnGet()
		{
			Path = Path?.Trim('/');
			History = new WikiHistoryModel
			{
				PageName = Path,
				Revisions = await _db.WikiPages
					.ForPage(Path)
					.ThatAreNotDeleted()
					.OrderBy(wp => wp.Revision)
					.Select(wp => new WikiHistoryModel.WikiRevisionModel
					{
						Revision = wp.Revision,
						CreateTimeStamp = wp.CreateTimeStamp,
						CreateUserName = wp.CreateUserName,
						MinorEdit = wp.MinorEdit,
						RevisionMessage = wp.RevisionMessage
					})
					.ToListAsync()
			};
		}
	}
}
