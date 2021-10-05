using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Legacy.Imports
{
	public static class ForumTopicFixer
	{
		public static void Fix(ApplicationDbContext db)
		{
			var topicIds = new[] { 3322, 786, 746, 761, 756, 736, 810, 765, 741, 743, 760, 739, 1362, 774, 781, 6750, 896, 1432, 1213, 834, 869, 750, 813, 795, 754, 772, 1087, 779, 775, 744, 764, 840, 749, 6749, 797, 853, 806, 758, 1077, 2138, 820, 759, 821, 729, 780, 805, 955, 2832, 977, 2561, 788, 839, 7387, 2819, 22843, 6765, 850, 3103, 799, 992, 747 };
			var topics = db.ForumTopics
				.Where(t => topicIds.Contains(t.Id))
				.ToList();

			var topicPosts = topics
				.Select(t => new ForumPost
				{
					TopicId = t.Id,
					PosterId = SiteGlobalConstants.TASVideoAgentId,
					Text = SiteGlobalConstants.NewSubmissionPost + $"[submission]{t.SubmissionId}[/submission]",
					EnableBbCode = true,
					PosterMood = ForumPostMood.Normal,
					CreateUserName = SiteGlobalConstants.TASVideoAgent,
					LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
					CreateTimestamp = t.CreateTimestamp,
					LastUpdateTimestamp = t.LastUpdateTimestamp
				})
				.ToList();

			var columns = new[]
			{
				nameof(ForumPost.TopicId),
				nameof(ForumPost.PosterId),
				nameof(ForumPost.IpAddress),
				nameof(ForumPost.Subject),
				nameof(ForumPost.Text),
				nameof(ForumPost.CreateTimestamp),
				nameof(ForumPost.CreateUserName),
				nameof(ForumPost.LastUpdateTimestamp),
				nameof(ForumPost.LastUpdateUserName),
				nameof(ForumPost.EnableHtml),
				nameof(ForumPost.EnableBbCode),
				nameof(ForumPost.PosterMood)
			};

			topicPosts.BulkInsert(columns, nameof(ApplicationDbContext.ForumPosts));
		}
	}
}
