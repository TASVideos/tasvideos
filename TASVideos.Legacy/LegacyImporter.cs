using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TASVideos.Data;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Forum.Entity;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Imports;

namespace TASVideos.Legacy
{
	public interface ILegacyImporter
	{
		void RunLegacyImport();
	}

	public class LegacyImporter : ILegacyImporter
	{
		private readonly IWebHostEnvironment _env;
		private readonly ApplicationDbContext _db;
		private readonly NesVideosSiteContext _legacySiteDb;
		private readonly NesVideosForumContext _legacyForumDb;
		private readonly ILogger<LegacyImporter> _logger;

		private static readonly Dictionary<string, long> ImportDurations = new ();

		public LegacyImporter(
			IWebHostEnvironment env,
			ApplicationDbContext db,
			NesVideosSiteContext legacySiteDb,
			NesVideosForumContext legacyForumDb,
			ILogger<LegacyImporter> logger)
		{
			_env = env;
			_db = db;
			_legacySiteDb = legacySiteDb;
			_legacyForumDb = legacyForumDb;
			_logger = logger;
		}

		public void RunLegacyImport()
		{
			_logger.LogInformation("Beginning Import");

			string connectionStr = _db.Database.GetConnectionString();
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			// Since we are using this database in a read-only way, set no tracking globally
			// To speed up query executions
			_legacySiteDb.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
			_legacyForumDb.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

			var stopwatch = Stopwatch.StartNew();
			SqlBulkImporter.BeginImport(connectionStr, _db.Database.IsSqlServer());
			Run("Tags", () => TagImporter.Import(_legacySiteDb));
			Run("Roms", () => RomImporter.Import(_legacySiteDb));
			Run("Games", () => GameImporter.Import(_legacySiteDb));
			Run("GameGroup", () => GameGroupImporter.Import(_legacySiteDb));
			Run("GameGenre", () => GameGenreImport.Import(_legacySiteDb));
			Run("RamAddresses", () => RamAddressImporter.Import(_db, _legacySiteDb));

			Run("Users", () => UserImporter.Import(_db, _legacySiteDb, _legacyForumDb));
			Run("UserMaintenanceLogs", () => UserMaintenanceLogImporter.Import(_db, _legacySiteDb));
			Run("UserDisallows", () => DisallowImporter.Import(_legacyForumDb));
			Run("Award", () => AwardImporter.Import(_db, _legacySiteDb));

			Run("Forum Categories", () => ForumCategoriesImporter.Import(_legacyForumDb));
			Run("Forums", () => ForumImporter.Import(_legacyForumDb));
			Run("Forum Topics", () => ForumTopicImporter.Import(_legacyForumDb));
			Run("Forum Posts", () => ForumPostsImporter.Import(_legacyForumDb));
			Run("Forum Private Messages", () => ForumPrivateMessagesImporter.Import(_legacyForumDb));
			Run("Forum Polls", () => ForumPollImporter.Import(_db, _legacyForumDb));

			// We don't want to copy these to other environments, as they can cause users to get unwanted emails
			if (_env.IsProduction())
			{
				Run("Forum Topic Watch", () => ForumTopicWatchImporter.Import(_legacyForumDb));
			}

			Run("Wiki", () => WikiImporter.Import(_legacySiteDb));
			Run("WikiCleanup", () => WikiPageCleanup.Fix(_db, _legacySiteDb));
			Run("WikiReferral", () => WikiReferralGenerator.Generate(_db));
			Run("Submissions", () => SubmissionImporter.Import(_db, _legacySiteDb));
			Run("Submissions Framerate", () => SubmissionFrameRateImporter.Import(_db));
			Run("Publications", () => PublicationImporter.Import(_db, _legacySiteDb));
			Run("PublicationUrls", () => PublicationUrlImporter.Import(_legacySiteDb));
			Run("Publication Ratings", () => PublicationRatingImporter.Import(_db, _legacySiteDb));
			Run("Publication Flags", () => PublicationFlagImporter.Import(_legacySiteDb));
			Run("Publication Tags", () => PublicationTagImporter.Import(_db, _legacySiteDb));
			Run("Published Author Generator", () => PublishedAuthorGenerator.Generate(_db));
			Run("Publication Maintenance Logs", () => PublicationMaintenanceLogImporter.Import(_db, _legacySiteDb));

			Run("User files", () => UserFileImporter.Import(_db, _legacySiteDb));

			var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
			stopwatch.Stop();
			_logger.LogInformation($"Import finished. Total time: {elapsedMilliseconds / 1000.0} seconds");
			_logger.LogInformation("Import breakdown:");
			foreach ((var key, long value) in ImportDurations)
			{
				_logger.LogInformation($"{key}: {value}");
			}
		}

		private static void Run(string name, Action import)
		{
			var stopwatch = Stopwatch.StartNew();
			try
			{
				import();
			}
			catch (Exception ex)
			{
				throw new ImportException(name, ex);
			}

			ImportDurations.Add($"{name} import", stopwatch.ElapsedMilliseconds);
			stopwatch.Stop();
		}

		public class ImportException : Exception
		{
			public ImportException(string importStep, Exception innerException)
				: base($"Exception at import step: {importStep}", innerException)
			{
				ImportStep = importStep;
			}

			public string ImportStep { get; }
		}
	}
}
