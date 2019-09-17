using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Profile.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class WatchedTopicsModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ITopicWatcher _topicWatcher;

		public WatchedTopicsModel(ApplicationDbContext db, ITopicWatcher topicWatcher)
		{
			_db = db;
			_topicWatcher = topicWatcher;
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
			await _topicWatcher.UnwatchTopic(topicId, User.GetUserId());
			return RedirectToPage("WatchedTopics");
		}
	}
}
