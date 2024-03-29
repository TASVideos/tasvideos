﻿namespace TASVideos.Pages.Profile;

[Authorize]
public class WatchedTopicsModel(ITopicWatcher topicWatcher) : BasePageModel
{
	public ICollection<WatchedTopic> Watches { get; set; } = [];

	public async Task OnGet()
	{
		Watches = await topicWatcher.UserWatches(User.GetUserId());
	}

	public async Task<IActionResult> OnPostStopWatching(int topicId)
	{
		await topicWatcher.UnwatchTopic(topicId, User.GetUserId());
		return BasePageRedirect("WatchedTopics");
	}

	public async Task<IActionResult> OnPostStopAllWatching()
	{
		await topicWatcher.UnwatchAllTopics(User.GetUserId());
		return BasePageRedirect("WatchedTopics");
	}
}
