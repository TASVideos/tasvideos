using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Profile.Models;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class WatchedTopicsModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public WatchedTopicsModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public IEnumerable<WatchedTopicEntry> Watches { get; set; } = new List<WatchedTopicEntry>();

		public async Task OnGet()
		{
			Watches = await _db
				.ForumTopicWatches
				.ForUser(User.GetUserId())
				.Select(tw => new WatchedTopicEntry
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

		public async Task<IActionResult> OnPostStopWatching(int topicId)
		{
			try
			{
				var userId = User.GetUserId();
				var watch = await _db.ForumTopicWatches
					.SingleOrDefaultAsync(tw => tw.UserId == userId && tw.ForumTopicId == topicId);
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
