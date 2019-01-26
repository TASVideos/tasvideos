using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class ForumTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly AwardTasks _awardTasks;
		private readonly IEmailService _emailService;

		public ForumTasks(
			ApplicationDbContext db,
			AwardTasks awardTasks,
			IEmailService emailService)
		{
			_db = db;
			_awardTasks = awardTasks;
			_emailService = emailService;
		}

		/// <summary>
		/// Returns the position of post is in its parent topic
		/// If a post with the given id can not be found, null is returned
		/// </summary>
		public async Task<PostPositionModel> GetPostPosition(int postId, bool seeRestricted)
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

		/// <summary>
		/// Creates a new <see cref="ForumTopic" /> and the first <see cref="ForumPost"/> of that topic
		/// </summary>
		/// <returns>
		/// The id of the newly created <see cref="ForumTopic" />
		/// If a topic could not be created, returns null
		/// </returns>
		public async Task<ForumTopic> CreateTopic(int forumId, TopicCreatePostModel model, User user, string ipAddress)
		{
			var topic = new ForumTopic
			{
				Type = model.Type,
				Title = model.Title,
				PosterId = user.Id,
				Poster = user,
				ForumId = forumId
			};

			_db.ForumTopics.Add(topic);
			await _db.SaveChangesAsync();

			var forumPostModel = new ForumPostModel
			{
				Subject = null,
				Text = model.Post
			};

			await CreatePost(topic.Id, forumPostModel, user, ipAddress);
			await WatchTopic(topic.Id, user.Id, canSeeRestricted: true);
			return topic;
		}

		public async Task<int> CreatePost(int topicId, ForumPostModel model, User user, string ipAddress)
		{
			var forumPost = new ForumPost
			{
				TopicId = topicId,
				PosterId = user.Id,
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
			await WatchTopic(topicId, user.Id, canSeeRestricted: true);
			return forumPost.Id;
		}

		/// <summary>
		/// Results the vote results of a given <see cref="ForumPoll"/> including
		/// the voters and which options they voted for
		/// </summary>
		public async Task<PollResultModel> GetPollResults(int pollId)
		{
			var poll = await _db.ForumPolls
				.Include(p => p.Topic)
				.Include(p => p.PollOptions)
				.ThenInclude(po => po.Votes)
				.ThenInclude(v => v.User)
				.SingleOrDefaultAsync(p => p.Id == pollId);

			if (poll == null)
			{
				return null;
			}

			return new PollResultModel
			{
				TopicTitle = poll.Topic.Title,
				TopicId = poll.TopicId,
				PollId = pollId,
				Question = poll.Question,
				Votes = poll.PollOptions
					.SelectMany(p => p.Votes)
					.Select(v => new PollResultModel.VoteResult
					{
						UserId = v.UserId,
						UserName = v.User.UserName,
						Ordinal = v.PollOption.Ordinal,
						OptionText = v.PollOption.Text,
						CreateTimestamp = v.CreateTimestamp,
						IpAddress = v.IpAddress
					})
			};
		}

		/// <summary>
		/// If a user is watching this topic, this marks the topic
		/// as not notified, at which point, any new post will cause a notification
		/// </summary>
		public async Task MarkTopicAsUnNotifiedForUser(int userId, int topicId)
		{
			var watchedTopic = await _db.ForumTopicWatches
				.SingleOrDefaultAsync(w => w.UserId == userId && w.ForumTopicId == topicId);

			if (watchedTopic != null && watchedTopic.IsNotified)
			{
				watchedTopic.IsNotified = false;
				await _db.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Should be called when a new post is created in a topic
		/// Will notify all users watching the topic and mark the IsNotified flag accordingly
		/// </summary>
		public async Task NotifyWatchedTopics(int topicId, int posterId)
		{
			var watches = await _db.ForumTopicWatches
				.Include(w => w.User)
				.Where(w => w.ForumTopicId == topicId)
				.Where(w => w.UserId != posterId)
				.Where(w => !w.IsNotified)
				.ToListAsync();

			if (watches.Any())
			{
				await _emailService.SendTopicNotification(watches.Select(w => w.User.Email));

				foreach (var watch in watches)
				{
					watch.IsNotified = true;
				}

				await _db.SaveChangesAsync();
			}
		}

		public async Task WatchTopic(int topicId, int userId, bool canSeeRestricted)
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

		public async Task UnwatchTopic(int topicId, int userId, bool canSeeRestricted)
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
