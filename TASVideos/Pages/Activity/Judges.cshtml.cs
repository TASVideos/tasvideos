using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Pages.Activity.Model;

namespace TASVideos.Pages.Activity
{
	[AllowAnonymous]
	public class JudgesModel : PageModel
	{
		private readonly ApplicationDbContext _db;

		public JudgesModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public string UserName { get; set; }

		public ICollection<SubmissionEntryModel> Submissions { get; set; } = new List<SubmissionEntryModel>();

		public async Task OnGet()
		{
			Submissions = await _db.Submissions
				.Where(s => s.JudgeId.HasValue)
				.Where(s => s.Judge.UserName == UserName)
				.Select(s => new SubmissionEntryModel
				{
					Id = s.Id,
					CreateTimeStamp = s.CreateTimeStamp,
					Title = s.Title,
					Status = s.Status
				})
				.ToListAsync();
		}
	}
}
