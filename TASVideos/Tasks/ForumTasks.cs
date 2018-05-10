using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.ViewComponents;

namespace TASVideos.Tasks
{
	public class ForumTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly AwardTasks _awardTasks;

		public ForumTasks(
			ApplicationDbContext db,
			AwardTasks awardTasks)
		{
			_db = db;
			_awardTasks = awardTasks;
		}

		/// <summary>
		/// Returns data necessary for the Forum/Index page
		/// </summary>
		public async Task<ForumIndexModel> GetForumIndex()
		{
			return new ForumIndexModel
			{
				Categories = await _db.ForumCategories
					.Include(c => c.Forums)
					.ToListAsync()
			};
		}

		/// <summary>
		/// Returns a forum and topics for the given id
		/// For the purpose of display
		/// </summary>
		public async Task<ForumModel> GetForumForDisplay(ForumRequest paging)
		{
			var model = await _db.Forums
				.Select(f => new ForumModel
				{
					Id = f.Id,
					Name = f.Name,
					Description = f.Description
				})
				.SingleOrDefaultAsync(f => f.Id == paging.Id);

			if (model == null)
			{
				return null;
			}

			model.Topics = _db.ForumTopics
				.Where(ft => ft.ForumId == paging.Id)
				.Select(ft => new ForumModel.ForumTopicEntry
				{
					Id = ft.Id,
					Title = ft.Title,
					CreateUserName = ft.CreateUserName,
					CreateTimestamp = ft.CreateTimeStamp,
					Type = ft.Type,
					Views = ft.Views,
					PostCount = ft.ForumPosts.Count,
					LastPost = ft.ForumPosts.Max(fp => fp.CreateTimeStamp)
				})
				.OrderByDescending(ft => ft.Type == ForumTopicType.Sticky)
				.ThenByDescending(ft => ft.Type == ForumTopicType.Announcement)
				.ThenByDescending(ft => ft.LastPost)
				.PageOf(_db, paging);

			return model;
		}

		/// <summary>
		/// Displays a page of posts for the given topic
		/// </summary>
		public async Task<ForumTopicModel> GetTopicForDisplay(TopicRequest paging)
		{
			var model = await _db.ForumTopics
				.Select(t => new ForumTopicModel
				{
					Id = t.Id,
					Title = t.Title
				})
				.SingleOrDefaultAsync(t => t.Id == paging.Id);

			if (model == null)
			{
				return null;
			}

			model.Posts = _db.ForumPosts
				.Where(p => p.TopicId == paging.Id)
				.Select(p => new ForumTopicModel.ForumPostEntry
				{
					Id = p.Id,
					EnableHtml = p.EnableHtml,
					EnableBbCode = p.EnableBbCode,
					PosterId = p.PosterId,
					CreateTimestamp = p.CreateTimeStamp,
					PosterName = p.Poster.UserName,
					PosterAvatar = p.Poster.Avatar,
					PosterLocation = p.Poster.From,
					PosterRoles = p.Poster.UserRoles
						.Where(ur => !ur.Role.IsDefault)
						.Select(ur => ur.Role.Name),
					PosterJoined = p.Poster.CreateTimeStamp,
					PosterPostCount = p.Poster.Posts.Count,
					Text = p.Text,
					Subject = p.Subject,
					Signature = p.Poster.Signature
				})
				.OrderBy(p => p.CreateTimestamp)
				.PageOf(_db, paging);

			foreach (var post in model.Posts)
			{
				post.Awards = await _awardTasks.GetAllAwardsForUser(post.PosterId);
			}

			return model;
		}

		// TODO: document
		public async Task<IEnumerable<TopicFeedModel.TopicPost>> GetTopicFeed(int topicId, int limit)
		{
			return await _db.ForumPosts
				.Where(p => p.TopicId == topicId)
				.Select(p => new TopicFeedModel.TopicPost
				{
					Id = p.Id,
					Text = p.Text,
					Subject = p.Subject,
					PosterName = p.Poster.UserName,
					PostTime = p.CreateTimeStamp
				})
				.OrderByDescending(p => p.PostTime)
				.Take(limit)
				.ToListAsync();
		}

		public async Task<string> GetPostText(int postId)
		{
			return await _db.ForumPosts
				.Where(p => p.Id == postId)
				.Select(p => p.Text)
				.SingleOrDefaultAsync();
		}

		public async Task<TopicCreateModel> GetTopicCreateData(int forumId)
		{
			var forum = await _db.Forums.SingleOrDefaultAsync(f => f.Id == forumId);

			if (forum == null)
			{
				return null;
			}

			return new TopicCreateModel
			{
				ForumId = forumId,
				ForumName = forum.Name
			};
		}

		/// <summary>
		/// Returns necessary data to display on the create post screen
		/// </summary>
		public async Task<ForumPostCreateModel> GetCreatePostData(int topicId, User user, int? postId)
		{
			var topic = await _db.ForumTopics
				.SingleOrDefaultAsync(t => t.Id == topicId);

			if (topic == null)
			{
				return null;
			}

			var model = new ForumPostCreateModel
			{
				TopicId = topicId,
				TopicTitle = topic.Title
			};

			if (postId.HasValue)
			{
				var post = await _db.ForumPosts
				.Include(p => p.Poster)
				.Where(p => p.Id == postId)
				.SingleOrDefaultAsync();

				model.Post = $"[quote=\"{post.Poster.UserName}\"]{post.Text}[/quote]";
			}

			return model;
		}

		public async Task CreatePost(ForumPostModel model, User user, string ipAddress)
		{
			var forumPost = new ForumPost
			{
				TopicId = model.TopicId,
				PosterId = user.Id,
				IpAddress = ipAddress,
				Subject = model.Subject,
				Text = model.Post,

				// TODO: check for bbcode and if none, set this to false?
				// For now we are not giving the user choices
				EnableHtml = false,
				EnableBbCode = true
			};

			_db.ForumPosts.Add(forumPost);
			await _db.SaveChangesAsync();
		}
	}
}
