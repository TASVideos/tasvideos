using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Pages.Activity.Model;

namespace TASVideos.Pages.Activity
{
	[AllowAnonymous]
	public class IndexModel : PageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public IEnumerable<ActivitySummaryModel> Judges { get; set; } = new List<ActivitySummaryModel>();
		public IEnumerable<ActivitySummaryModel> Publishers { get; set; } = new List<ActivitySummaryModel>();

		public async Task OnGet()
		{
			Judges = await _db.Submissions
				.Where(s => s.JudgeId.HasValue)
				.GroupBy(s => s.Judge.UserName)
				.Select(s => new ActivitySummaryModel
				{
					UserName = s.Key,
					Count = s.Count()
				})
				.ToListAsync();

			Publishers = await _db.Publications
				.GroupBy(p => p.CreateUserName)
				.Select(p => new ActivitySummaryModel
				{
					UserName = p.Key,
					Count = p.Count()
				})
				.ToListAsync();
		}
	}
}
