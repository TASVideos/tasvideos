using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Forum.Entity;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Imports;

namespace TASVideos.Legacy
{
	public static class LegacyImporter
	{
		public static void RunLegacyImport(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext,
			NesVideosForumContext legacyForumContext)
		{
			// Since we are using this database in a read-only way, set no tracking globally
			// To speed up query executions
			legacySiteContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
			legacyForumContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

			TagImporter.Import(context, legacySiteContext);
			RomImporter.Import(context, legacySiteContext);
			GameImporter.Import(context, legacySiteContext);
			UserImporter.Import(context, legacySiteContext, legacyForumContext);
			AwardImporter.Import(context, legacySiteContext);

			ForumCategoriesImporter.Import(context, legacyForumContext);
			ForumImporter.Import(context, legacyForumContext);
			ForumTopicImporter.Import(context, legacyForumContext);
			ForumPostsImporter.Import(context, legacyForumContext);
			ForumPrivateMessagesImporter.Import(context, legacyForumContext);
			ForumPollImporter.Import(context, legacyForumContext);

			//WikiImporter.Import(context, legacySiteContext);
			//SubmissionImporter.Import(context, legacySiteContext);
			//PublicationImporter.Import(context, legacySiteContext);
			//PublicationRatingImporter.Import(context, legacySiteContext);
			//PublicationFlagImporter.Import(context, legacySiteContext);
		}
	}
}
