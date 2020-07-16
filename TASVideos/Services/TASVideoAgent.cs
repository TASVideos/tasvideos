using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Services
{
	public interface ITASVideoAgent
	{
		Task PostSubmissionTopic(int submissionId, string postTitle);
		Task PostSubmissionPublished(int submissionId, int publicationId);
	}

	public class TASVideoAgent : ITASVideoAgent
	{
		private readonly ApplicationDbContext _db;

		public TASVideoAgent(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task PostSubmissionTopic(int submissionId, string title)
		{
			var poll = new ForumPoll
			{
				CreateUserName = SiteGlobalConstants.TASVideoAgent,
				Question = SiteGlobalConstants.PollQuestion,
				PollOptions = new[]
				{
					new ForumPollOption { Text = SiteGlobalConstants.PollOptionNo, Ordinal = 0 },
					new ForumPollOption { Text = SiteGlobalConstants.PollOptionYes, Ordinal = 1 },
					new ForumPollOption { Text = SiteGlobalConstants.PollOptionsMeh, Ordinal = 2 }
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
				PageName = LinkConstants.SubmissionWikiPage + submissionId,
				Poll = poll
			};

			// Create first post
			var post = new ForumPost
			{
				CreateUserName = SiteGlobalConstants.TASVideoAgent,
				LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
				Topic = topic,
				PosterId = SiteGlobalConstants.TASVideoAgentId,
				Subject = title,
				Text = SiteGlobalConstants.NewSubmissionPost + $"<a href=\"/{submissionId}S\">{title}</a>",
				EnableHtml = true,
				EnableBbCode = false,
				PosterMood = ForumPostMood.Normal
			};

			_db.ForumPolls.Add(poll);
			_db.ForumTopics.Add(topic);
			_db.ForumPosts.Add(post);
			await _db.SaveChangesAsync();

			poll.TopicId = topic.Id;
			poll.LastUpdateUserName = SiteGlobalConstants.TASVideoAgent; // Necessary for LastUpdatedUser to not change
			await _db.SaveChangesAsync();
		}

		public async Task PostSubmissionPublished(int submissionId, int publicationId)
		{
			var topic = await _db.ForumTopics.SingleOrDefaultAsync(f => f.PageName == LinkConstants.SubmissionWikiPage + submissionId);

			// We intentionally silently fail here.
			// Otherwise we would leave publication in a partial state
			// which would be worse than a missing forum post
			if (topic != null)
			{
				_db.ForumPosts.Add(new ForumPost
				{
					TopicId = topic.Id,
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
