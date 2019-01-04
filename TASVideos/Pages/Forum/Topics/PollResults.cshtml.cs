using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.SeePollResults)]
	public class PollResultsModel : BasePageModel
	{
		private readonly ForumTasks _forumTasks;

		public PollResultsModel(
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_forumTasks = forumTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		public PollResultModel Poll { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Poll = await _forumTasks.GetPollResults(Id);

			if (Poll == null)
			{
				return NotFound();
			}

			Poll.Question = RenderPost(Poll.Question, true, false); // TODO: flags

			return Page();
		}
	}
}
