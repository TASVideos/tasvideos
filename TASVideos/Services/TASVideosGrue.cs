using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Services
{
	public interface ITASVideosGrue
	{
		Task PostSubmissionRejection(int submissionId);
	}

	public class TASVideosGrue : ITASVideosGrue
	{
		private readonly ApplicationDbContext _db;

		public TASVideosGrue(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task PostSubmissionRejection(int submissionId)
		{
			var topic = await _db.ForumTopics.SingleOrDefaultAsync(f => f.PageName == LinkConstants.SubmissionWikiPage + submissionId);

			// We intentionally silently fail here.
			// Otherwise we would leave submission rejection in a partial state
			// which would be worse than a missing forum post
			if (topic != null)
			{
				_db.ForumPosts.Add(new ForumPost
				{
					TopicId = topic.Id,
					CreateUserName = SiteGlobalConstants.TASVideosGrue,
					LastUpdateUserName = SiteGlobalConstants.TASVideosGrue,
					PosterId = SiteGlobalConstants.TASVideosGrueId,
					// TODO: different moods
					Text = "om, nom, nom... crunchy!",
					PosterMood = ForumPostMood.Normal
				});
				await _db.SaveChangesAsync();
			}
		}
	}
}
