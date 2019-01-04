using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.MoveTopics)]
	public class MoveModel : BasePageModel
	{
		private readonly ExternalMediaPublisher _publisher;
		private readonly ForumTasks _forumTasks;

		public MoveModel(
			ExternalMediaPublisher publisher,
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_publisher = publisher;
			_forumTasks = forumTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public MoveTopicModel Topic { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var seeRestricted = UserHas(PermissionTo.SeeRestrictedForums);
			if (!seeRestricted)
			{
				if (!await _forumTasks.TopicAccessible(Id, false))
				{
					return NotFound();
				}
			}

			Topic = await _forumTasks.GetTopicMoveModel(Id, seeRestricted);

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

			var result = await _forumTasks.MoveTopic(Id, Topic, UserHas(PermissionTo.SeeRestrictedForums));
			if (result)
			{
				var forum = await _forumTasks.GetForum(Topic.ForumId);
				_publisher.SendForum(
					forum.Restricted,
					$"Topic {Topic.TopicTitle} moved from {Topic.ForumName} to {forum.Name}",
					"",
					$"{BaseUrl}/Forum/Topics/{Id}");
			}

			return RedirectToPage("Index", new { Id });
		}
	}
}
