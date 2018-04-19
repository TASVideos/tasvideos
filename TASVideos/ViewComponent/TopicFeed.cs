using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Tasks;

namespace TASVideos.ViewComponents
{
    public class TopicFeed : ModuleComponentBase
	{
		private readonly ForumTasks _forumTasks;

		public TopicFeed(ForumTasks forumTasks)
		{
			_forumTasks = forumTasks;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			int limit = GetInt(pp, "l") ?? 5;
			int topicId = GetInt(pp, "t")
				?? throw new ArgumentException("the parameter t can not be null");

			var model = new TopicFeedModel
			{
				RightAlign = HasParam(pp, "right"),
				Heading = GetValueFor(pp, "heading"),
				HideContent = HasParam(pp, "hidecontent"),
				Posts = await _forumTasks.GetTopicFeed(topicId, limit)
			};

			return View(model);
		}
    }
}
