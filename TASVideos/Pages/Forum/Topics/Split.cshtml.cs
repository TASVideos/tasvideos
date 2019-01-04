using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.SplitTopics)]
	public class SplitModel : BasePageModel
	{
		private readonly ExternalMediaPublisher _publisher;
		private readonly UserManager<User> _userManager;
		private readonly ForumTasks _forumTasks;

		public SplitModel(
			ExternalMediaPublisher publisher,
			UserManager<User> userManager,
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_publisher = publisher;
			_userManager = userManager;
			_forumTasks = forumTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public SplitTopicModel Topic { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Topic = await _forumTasks.GetTopicForSplit(Id, UserHas(PermissionTo.SeeRestrictedForums));
			if (Topic == null)
			{
				return NotFound();
			}

			foreach (var post in Topic.Posts)
			{
				post.Text = RenderPost(post.Text, post.EnableBbCode, post.EnableHtml);
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var user = await _userManager.GetUserAsync(User);

			var result = await _forumTasks.SplitTopic(
				Id,
				Topic,
				UserHas(PermissionTo.SeeRestrictedForums),
				user);

			if (result == null)
			{
				return NotFound();
			}

			var topic = await _forumTasks.GetTopic(result.Value);
			_publisher.SendForum(
				topic.Forum.Restricted,
				$"Topic {topic.Forum.Name}: {topic.Title} SPLIT from {Topic.ForumName}: {Topic.Title}",
				"",
				$"{BaseUrl}/Forum/Topics/{topic.Id}");

			return RedirectToPage("Index", new { id = result.Value });
		}
	}
}
