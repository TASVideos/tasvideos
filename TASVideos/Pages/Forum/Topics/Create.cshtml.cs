using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.CreateForumTopics)]
	public class CreateModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly ExternalMediaPublisher _publisher;
		private readonly ForumTasks _forumTasks;

		public CreateModel(
			UserManager<User> userManager,
			ExternalMediaPublisher publisher,
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_publisher = publisher;
			_forumTasks = forumTasks;
		}

		[FromRoute]
		public int ForumId { get; set; }

		[BindProperty]
		public TopicCreatePostModel Topic { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Topic = await _forumTasks.GetCreateTopicData(ForumId, UserHas(PermissionTo.SeeRestrictedForums));
			if (Topic == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var forum = await _forumTasks.GetForum(ForumId);
			if (forum == null)
			{
				return NotFound();
			}

			if (forum.Restricted && !UserHas(PermissionTo.SeeRestrictedForums))
			{
				return NotFound();
			}

			var user = await _userManager.GetUserAsync(User);
			var topic = await _forumTasks.CreateTopic(ForumId, Topic, user, IpAddress.ToString());

			//// TODO: auto-add topic permission based on post count, also ability to vote
			
			_publisher.SendForum(
				forum.Restricted,
				$"New Topic by {User.Identity.Name} ({forum.ShortName}: {Topic.Title})",
				Topic.Post.CapAndEllipse(50),
				$"/Forum/Topic{topic.Id}");

			return RedirectToPage("Index", new { topic.Id });
		}
	}
}
