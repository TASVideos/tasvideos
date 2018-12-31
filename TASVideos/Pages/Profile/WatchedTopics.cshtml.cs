using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Tasks;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class WatchedTopicsModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly ApplicationDbContext _db;
		public WatchedTopicsModel(
			UserManager<User> userManager,
			ApplicationDbContext db,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_db = db;
		}

		// TODO: rename this model
		public IEnumerable<Models.WatchedTopicsModel> Watches { get; set; } = new List<Models.WatchedTopicsModel>();

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			Watches = await _db
				.ForumTopicWatches
				.ForUser(user.Id)
				.Select(tw => new Models.WatchedTopicsModel
				{
					TopicCreateTimeStamp = tw.ForumTopic.CreateTimeStamp,
					IsNotified = tw.IsNotified,
					ForumId = tw.ForumTopic.ForumId,
					ForumTitle = tw.ForumTopic.Forum.Name,
					TopicId = tw.ForumTopicId,
					TopicTitle = tw.ForumTopic.Title,
				})
				.ToListAsync();
		}

		public async Task<IActionResult> OnGetStopWatching(int topicId)
		{
			var user = await _userManager.GetUserAsync(User);
			try
			{
				var watch = await _db.ForumTopicWatches
					.SingleOrDefaultAsync(tw => tw.UserId == user.Id && tw.ForumTopicId == topicId);
				_db.ForumTopicWatches.Remove(watch);
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				// Do nothing
				// 1) if a watch is already removed, we are done
				// 2) if a watch was updated (for instance, someone posted in the topic),
				//		there isn't much we can do other than reload the page anyway with an error
				//		An error would only be modestly helpful anyway, and wouldn't save clicks
				//		However, this would be an nice to have one day
			}

			return RedirectToPage("WatchedTopics");
		}
	}
}
