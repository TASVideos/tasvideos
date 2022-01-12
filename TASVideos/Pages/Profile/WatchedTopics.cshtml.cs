using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class WatchedTopicsModel : BasePageModel
	{
		private readonly ITopicWatcher _topicWatcher;

		public WatchedTopicsModel(ITopicWatcher topicWatcher)
		{
			_topicWatcher = topicWatcher;
		}

		public IEnumerable<WatchedTopic> Watches { get; set; } = new List<WatchedTopic>();

		public async Task OnGet()
		{
			Watches = await _topicWatcher.UserWatches(User.GetUserId());
		}

		public async Task<IActionResult> OnPostStopWatching(int topicId)
		{
			await _topicWatcher.UnwatchTopic(topicId, User.GetUserId());
			return BasePageRedirect("WatchedTopics");
		}

		public async Task<IActionResult> OnPostStopAllWatching()
		{
			await _topicWatcher.UnwatchAllTopics(User.GetUserId());
			return BasePageRedirect("WatchedTopics");
		}
	}
}
