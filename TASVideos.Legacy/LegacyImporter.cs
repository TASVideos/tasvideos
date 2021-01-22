using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TASVideos.Data;
using TASVideos.Legacy.Data.Forum;
using TASVideos.Legacy.Data.Forum.Entity;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Imports;

namespace TASVideos.Legacy
{
	public static class LegacyImporter
	{
		private static readonly Dictionary<string, long> ImportDurations = new ();

		public static void RunLegacyImport(
			IWebHostEnvironment env,
			ApplicationDbContext context,
			string connectionStr,
			NesVideosSiteContext legacySiteContext,
			NesVideosForumContext legacyForumContext)
		{
			// Since we are using this database in a read-only way, set no tracking globally
			// To speed up query executions
			legacySiteContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
			legacyForumContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

			var stopwatch = Stopwatch.StartNew();

			Run("Tags", () => TagImporter.Import(connectionStr, legacySiteContext));
			Run("Roms", () => RomImporter.Import(connectionStr, legacySiteContext));
			Run("Games", () => GameImporter.Import(connectionStr, legacySiteContext));
			Run("GameGroup", () => GameGroupImporter.Import(connectionStr, legacySiteContext));
			Run("GameGenre", () => GameGenreImport.Import(connectionStr, legacySiteContext));
			Run("RamAddresses", () => RamAddressImporter.Import(connectionStr, context, legacySiteContext));

			Run("Users", () => UserImporter.Import(connectionStr, context, legacySiteContext, legacyForumContext));
			Run("UserDisallows", () => DisallowImporter.Import(connectionStr, legacyForumContext));
			Run("Award", () => AwardImporter.Import(connectionStr, context, legacySiteContext));

			Run("Forum Categories", () => ForumCategoriesImporter.Import(connectionStr, legacyForumContext));
			Run("Forums", () => ForumImporter.Import(connectionStr, legacyForumContext));
			Run("Forum Topics", () => ForumTopicImporter.Import(connectionStr, legacyForumContext));
			Run("Forum Posts", () => ForumPostsImporter.Import(connectionStr, legacyForumContext));
			Run("Forum Private Messages", () => ForumPrivateMessagesImporter.Import(connectionStr, legacyForumContext));
			Run("Forum Polls", () => ForumPollImporter.Import(connectionStr, context, legacyForumContext));

			// We don't want to copy these to other environments, as they can cause users to get unwanted emails
			if (env.IsProduction())
			{
				Run("Forum Topic Watch", () => ForumTopicWatchImporter.Import(connectionStr, legacyForumContext));
			}

			Run("Wiki", () => WikiImporter.Import(connectionStr, legacySiteContext));
			Run("WikiCleanup", () => WikiPageCleanup.Fix(context, legacySiteContext));
			Run("WikiReferral", () => WikiReferralGenerator.Generate(connectionStr, context));
			Run("Submissions", () => SubmissionImporter.Import(connectionStr, context, legacySiteContext));
			Run("Submissions Framerate", () => SubmissionFrameRateImporter.Import(context));
			Run("Publications", () => PublicationImporter.Import(connectionStr, context, legacySiteContext));
			Run("PublicationUrls", () => PublicationUrlImporter.Import(connectionStr, legacySiteContext));
			Run("Publication Ratings", () => PublicationRatingImporter.Import(connectionStr, context, legacySiteContext));
			Run("Publication Flags", () => PublicationFlagImporter.Import(connectionStr, legacySiteContext));
			Run("Publication Tags", () => PublicationTagImporter.Import(connectionStr, context, legacySiteContext));
			Run("Published Author Generator", () => PublishedAuthorGenerator.Generate(connectionStr, context));

			Run("User files", () => UserFileImporter.Import(connectionStr, context, legacySiteContext));

			var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
			stopwatch.Stop();
			Console.WriteLine($"Import finished. Total time: {elapsedMilliseconds / 1000.0} seconds");
			Console.WriteLine("Import breakdown:");
			foreach ((var key, long value) in ImportDurations)
			{
				Console.WriteLine($"{key}: {value}");
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
