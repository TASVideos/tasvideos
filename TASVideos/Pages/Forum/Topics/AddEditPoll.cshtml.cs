using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.CreateForumPolls)]
	public class AddEditPollModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public AddEditPollModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int TopicId { get; set; }

		public string TopicTitle { get; set; }

		public PollResultModel Poll { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var topic = await _db.ForumTopics
				.Include(t => t.Poll)
				.SingleOrDefaultAsync();

			if (topic == null)
			{
				return NotFound();
			}

			return Page();
		}
	}
}
