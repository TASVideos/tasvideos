using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Posts
{
	[RequirePermission(PermissionTo.CreateForumPosts)]
	public class CreateModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly ForumTasks _forumTasks;
		private readonly ExternalMediaPublisher _publisher;

		public CreateModel(
			UserManager<User> userManager,
			ForumTasks forumTasks,
			ExternalMediaPublisher publisher,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_userManager = userManager;
			_forumTasks = forumTasks;
			_publisher = publisher;
		}

		[FromRoute]
		public int TopicId { get; set; }

		[FromQuery]
		public int? QuoteId { get; set; }

		[BindProperty]
		public ForumPostCreateModel Post { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Post = await _forumTasks.GetCreatePostData(TopicId, QuoteId, UserHas(PermissionTo.SeeRestrictedForums));

			if (Post == null)
			{
				return NotFound();
			}

			if (Post.IsLocked && !UserHas(PermissionTo.PostInLockedTopics))
			{
				return AccessDenied();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			var user = await _userManager.GetUserAsync(User);
			if (!ModelState.IsValid)
			{
				// We have to consider direct posting to this call, including "over-posting",
				// so all of this logic is necessary
				var isLocked = await _forumTasks.IsTopicLocked(TopicId);
				if (isLocked && !UserHas(PermissionTo.PostInLockedTopics))
				{
					return AccessDenied();
				}

				Post = new ForumPostCreateModel
				{
					TopicTitle = Post.TopicTitle,
					Subject = Post.Subject,
					Text = Post.Text,
					IsLocked = isLocked,
					UserAvatar = user.Avatar,
					UserSignature = user.Signature
				};

				return Page();
			}

			var topic = await _forumTasks.GetTopic(TopicId);
			if (topic == null)
			{
				return NotFound();
			}

			if (topic.Forum.Restricted && !UserHas(PermissionTo.SeeRestrictedForums))
			{
				return NotFound();
			}

			if (topic.IsLocked && !UserHas(PermissionTo.PostInLockedTopics))
			{
				return AccessDenied();
			}

			var id = await _forumTasks.CreatePost(TopicId, Post, user, IpAddress.ToString());

			_publisher.SendForum(
				topic.Forum.Restricted,
				$"New reply by {user.UserName} ({topic.Forum.ShortName}: {topic.Title}) ({Post.Subject})",
				$"{Post.TopicTitle} ({Post.Subject})",
				$"{BaseUrl}/p/{id}#{id}");

			await _forumTasks.NotifyWatchedTopics(TopicId, user.Id);

			return RedirectToPage("/Forum/Topic", new { Id = TopicId });
		}
	}
}
