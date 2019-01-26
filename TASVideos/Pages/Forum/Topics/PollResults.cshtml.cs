using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.SeePollResults)]
	public class PollResultsModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public PollResultsModel(
			ApplicationDbContext db,
			UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		public PollResultModel Poll { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Poll = await _db.ForumPolls
				.Where(p => p.Id == Id)
				.Select(p => new PollResultModel
				{
					TopicTitle = p.Topic.Title,
					TopicId = p.TopicId,
					Question = p.Question,
					Votes = p.PollOptions
						.SelectMany(po => po.Votes)
						.Select(v => new PollResultModel.VoteResult
						{
							UserId = v.UserId,
							UserName = v.User.UserName,
							Ordinal = v.PollOption.Ordinal,
							OptionText = v.PollOption.Text,
							CreateTimestamp = v.CreateTimestamp,
							IpAddress = v.IpAddress
						})
						.ToList()
				})
				.SingleOrDefaultAsync();

			if (Poll == null)
			{
				return NotFound();
			}

			Poll.Question = RenderPost(Poll.Question, true, false); // TODO: flags

			return Page();
		}
	}
}
