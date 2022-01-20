using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Core.Services
{
	public interface ITASVideoAgent
	{
		Task<int> PostSubmissionTopic(int submissionId, string postTitle);
		Task PostSubmissionPublished(int submissionId, int publicationId);
	}

	internal class TASVideoAgent : ITASVideoAgent
	{
		private readonly ApplicationDbContext _db;
		private readonly IForumService _forumService;

		public TASVideoAgent(ApplicationDbContext db, IForumService forumService)
		{
			_db = db;
			_forumService = forumService;
		}

		public async Task<int> PostSubmissionTopic(int submissionId, string title)
		{
			var poll = new ForumPoll
			{
				CreateUserName = SiteGlobalConstants.TASVideoAgent,
				Question = SiteGlobalConstants.PollQuestion,
				PollOptions = new ForumPollOption[]
				{
					new () { Text = SiteGlobalConstants.PollOptionNo, Ordinal = 0 },
					new () { Text = SiteGlobalConstants.PollOptionYes, Ordinal = 1 },
					new () { Text = SiteGlobalConstants.PollOptionsMeh, Ordinal = 2 }
				}
			};

			// Create Topic in workbench
			var topic = new ForumTopic
			{
				CreateUserName = SiteGlobalConstants.TASVideoAgent,
				LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
				ForumId = ForumConstants.WorkBenchForumId,
				Title = title,
				PosterId = SiteGlobalConstants.TASVideoAgentId,
				SubmissionId = submissionId,
				Poll = poll
			};

			// Create first post
			var post = new ForumPost
			{
				CreateUserName = SiteGlobalConstants.TASVideoAgent,
				LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
				Topic = topic,
				ForumId = ForumConstants.WorkBenchForumId,
				PosterId = SiteGlobalConstants.TASVideoAgentId,
				Text = SiteGlobalConstants.NewSubmissionPost + $"[submission]{submissionId}[/submission]",
				EnableHtml = false,
				EnableBbCode = true,
				PosterMood = ForumPostMood.Normal
			};

			_db.ForumPolls.Add(poll);
			_db.ForumTopics.Add(topic);
			_db.ForumPosts.Add(post);
			await _db.SaveChangesAsync();

			poll.TopicId = topic.Id;
			poll.LastUpdateUserName = SiteGlobalConstants.TASVideoAgent; // Necessary for LastUpdatedUser to not change
			await _db.SaveChangesAsync();

			_forumService.CacheLatestPost(
				ForumConstants.WorkBenchForumId,
				topic.Id,
				new LatestPost(post.Id, post.CreateTimestamp, SiteGlobalConstants.TASVideoAgent));

			return topic.Id;
		}

		public async Task PostSubmissionPublished(int submissionId, int publicationId)
		{
			var topic = await _db.ForumTopics.SingleOrDefaultAsync(f => f.SubmissionId == submissionId);

			// We intentionally silently fail here.
			// Otherwise we would leave publication in a partial state
			// which would be worse than a missing forum post
			if (topic is not null)
			{
				topic.ForumId = SiteGlobalConstants.PublishedMoviesForumId;
				_db.ForumPosts.Add(new ForumPost
				{
					TopicId = topic.Id,
					ForumId = topic.ForumId,
					CreateUserName = SiteGlobalConstants.TASVideoAgent,
					LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
					PosterId = SiteGlobalConstants.TASVideoAgentId,
					EnableBbCode = true,
					EnableHtml = false,
					Subject = SiteGlobalConstants.NewPublicationPostSubject,
					Text = SiteGlobalConstants.NewPublicationPost.Replace("{PublicationId}", publicationId.ToString()),
					PosterMood = ForumPostMood.Happy
				});
				await _db.SaveChangesAsync();
			}
		}
	}
}
