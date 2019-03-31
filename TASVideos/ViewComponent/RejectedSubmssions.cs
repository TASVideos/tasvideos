using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class RejectedSubmissions : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public RejectedSubmissions(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			var model = await _db.Submissions
				.ThatAreRejected()
				.Select(s => new RejectedSubmission
				{
					Id = s.Id,
					Title = s.Title,
					Reason = s.RejectionReasonId.HasValue
						? s.RejectionReason.DisplayName
						: "N/A"
				})
				.ToListAsync();
			return View(model);
		}
	}
}
