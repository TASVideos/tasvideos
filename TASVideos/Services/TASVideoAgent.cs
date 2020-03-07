using System.Threading.Tasks;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Services
{
	public interface ITASVideoAgent
	{
		Task PostSubmissionTopic(int submissionId, string postTitle);
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
				LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
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

			await _db.ForumPolls.AddAsync(poll);
			await _db.ForumTopics.AddAsync(topic);
			await _db.ForumPosts.AddAsync(post);
			await _db.SaveChangesAsync();

			poll.TopicId = topic.Id;
			await _db.SaveChangesAsync();
		}
	}
}
