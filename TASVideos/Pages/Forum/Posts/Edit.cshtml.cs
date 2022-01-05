using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum.Posts
{
	[RequirePermission(
		true,
		PermissionTo.SeeRestrictedForums,
		PermissionTo.CreateForumPosts,
		PermissionTo.DeleteForumPosts,
		PermissionTo.EditForumPosts)]
	public class EditModel : BaseForumModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public EditModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			ITopicWatcher watcher)
			: base(db, watcher)
		{
			_db = db;
			_publisher = publisher;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public ForumPostEditModel Post { get; set; } = new ();

		[BindProperty]
		[DisplayName("Minor Edit")]
		public bool MinorEdit { get; set; } = false;

		public async Task<IActionResult> OnGet()
		{
			var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			Post = await _db.ForumPosts
				.ExcludeRestricted(seeRestricted)
				.Where(p => p.Id == Id)
				.Select(p => new ForumPostEditModel
				{
					CreateTimestamp = p.CreateTimestamp,
					PosterId = p.PosterId,
					PosterName = p.Poster!.UserName,
					EnableBbCode = p.EnableBbCode,
					EnableHtml = p.EnableHtml,
					TopicId = p.TopicId ?? 0,
					TopicTitle = p.Topic!.Title,
					Subject = p.Subject,
					Text = p.Text,
					Mood = p.PosterMood
				})
				.SingleOrDefaultAsync();

			if (Post == null)
			{
				return NotFound();
			}

			var firstPostId = (await _db.ForumPosts
				.ForTopic(Post.TopicId)
				.OldestToNewest()
				.FirstAsync())
				.Id;

			Post.IsFirstPost = Id == firstPostId;

			if (!User.Has(PermissionTo.EditForumPosts)
				&& Post.PosterId != User.GetUserId())
			{
				return AccessDenied();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);

			var forumPost = await _db.ForumPosts
				.Include(p => p.Topic)
				.Include(p => p.Topic!.Forum)
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(p => p.Id == Id);

			if (forumPost == null)
			{
				return NotFound();
			}

			if (!User.Has(PermissionTo.EditForumPosts)
				&& forumPost.PosterId != User.GetUserId())
			{
				ModelState.AddModelError("", "Unable to edit post.");
				return Page();
			}

			if (!string.IsNullOrWhiteSpace(Post.TopicTitle))
			{
				var firstPostId = (await _db.ForumPosts
					.ForTopic(forumPost.Topic!.Id)
					.OldestToNewest()
					.FirstAsync())
					.Id;
				if (Id == firstPostId)
				{
					forumPost.Topic!.Title = Post.TopicTitle;
				}
			}

			forumPost.Subject = Post.Subject;
			forumPost.Text = Post.Text;
			forumPost.PosterMood = Post.Mood;

			var result = await ConcurrentSave(_db, $"Post {Id} edited", "Unable to edit post");
			if (result && !MinorEdit)
			{
				await _publisher.SendForum(
					forumPost.Topic!.Forum!.Restricted,
					$"Post edited by {User.Name()} ({forumPost.Topic.Forum.ShortName}: {forumPost.Topic.Title})",
					"",
					$"Forum/p/{Id}#{Id}",
					User.Name());
			}

			return BaseRedirect($"/forum/p/{Id}#{Id}");
		}

		public async Task<IActionResult> OnPostDelete()
		{
			var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			var post = await _db.ForumPosts
				.Include(p => p.Topic)
				.Include(p => p.Topic!.Forum)
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(p => p.Id == Id);

			if (post == null)
			{
				return NotFound();
			}

			if (!User.Has(PermissionTo.DeleteForumPosts))
			{
				// Check if last post
				var lastPost = _db.ForumPosts
					.ForTopic(post.TopicId ?? -1)
					.ByMostRecent()
					.First();

				bool isLastPost = lastPost.Id == post.Id;
				if (!isLastPost)
				{
					return NotFound();
				}
			}

			var postCount = await _db.ForumPosts.CountAsync(t => t.TopicId == post.TopicId);

			_db.ForumPosts.Remove(post);

			bool topicDeleted = false;
			if (postCount == 1)
			{
				var topic = await _db.ForumTopics.SingleAsync(t => t.Id == post.TopicId);
				_db.ForumTopics.Remove(topic);
				topicDeleted = true;
			}

			var result = await ConcurrentSave(_db, $"Post {Id} deleted", $"Unable to delete post {Id}");
			if (result)
			{
				await _publisher.SendForum(
					post.Topic!.Forum!.Restricted,
					$"Post DELETED by {User.Name()} ({post.Topic.Forum.ShortName}: {post.Topic.Title})",
					"",
					$"Forum/Topics/{post.Topic.Id}",
					User.Name());
			}

			return topicDeleted
				? BasePageRedirect("/Forum/Subforum/Index", new { id = post.Topic!.ForumId })
				: BasePageRedirect("/Forum/Topics/Index", new { id = post.TopicId });
		}
	}
}
