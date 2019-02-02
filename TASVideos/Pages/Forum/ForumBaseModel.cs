using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Models;
using TASVideos.Pages.Forum.Posts.Models;

namespace TASVideos.Pages.Forum
{
	public class BaseForumModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public BaseForumModel(ApplicationDbContext db)
		{
			_db = db;
		}

		protected async Task<PostPositionModel> GetPostPosition(int postId, bool seeRestricted)
		{
			var post = await _db.ForumPosts
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(p => p.Id == postId);

			if (post == null)
			{
				return null;
			}

			var posts = await _db.ForumPosts
				.ForTopic(post.TopicId ?? -1)
				.OldestToNewest()
				.ToListAsync();

			var position = posts.IndexOf(post);
			return new PostPositionModel
			{
				Page = (position / ForumConstants.PostsPerPage) + 1,
				TopicId = post.TopicId ?? 0
			};
		}

		protected async Task<int> CreatePost(int topicId, ForumPostModel model, int userId, string ipAddress)
		{
			var forumPost = new ForumPost
			{
				TopicId = topicId,
				PosterId = userId,
				IpAddress = ipAddress,
				Subject = model.Subject,
				Text = model.Text,

				// TODO: check for bbcode and if none, set this to false?
				// For now we are not giving the user choices
				EnableHtml = false,
				EnableBbCode = true
			};

			_db.ForumPosts.Add(forumPost);
			await _db.SaveChangesAsync();
			await WatchTopic(topicId, userId, canSeeRestricted: true);
			return forumPost.Id;
		}

		protected async Task WatchTopic(int topicId, int userId, bool canSeeRestricted)
		{
			var watch = await _db.ForumTopicWatches
				.ExcludeRestricted(canSeeRestricted)
				.SingleOrDefaultAsync(w => w.UserId == userId
				&& w.ForumTopicId == topicId);

			if (watch == null)
			{
				_db.ForumTopicWatches.Add(new ForumTopicWatch
				{
					UserId = userId,
					ForumTopicId = topicId
				});

				await _db.SaveChangesAsync();
			}
		}

		protected async Task UnwatchTopic(int topicId, int userId, bool canSeeRestricted)
		{
			var watch = await _db.ForumTopicWatches
				.ExcludeRestricted(canSeeRestricted)
				.SingleOrDefaultAsync(w => w.UserId == userId
				&& w.ForumTopicId == topicId);

			if (watch != null)
			{
				_db.ForumTopicWatches.Remove(watch);
				await _db.SaveChangesAsync();
			}
		}
	}
}
