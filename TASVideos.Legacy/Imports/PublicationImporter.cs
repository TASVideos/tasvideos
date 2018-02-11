using System;
using System.Data.SqlClient;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
    public class PublicationImporter
    {
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			// TODO: streaming url links
			// TODO: archive links
			// TODO: import screenshots and other media server files

			var legacyMovies = legacySiteContext.Movies.Where(m => m.Id > 0).ToList();
			var legacyMovieFiles = legacySiteContext.MovieFiles.ToList();
			var legacyMovieFileStorage = legacySiteContext.MovieFileStorage.ToList();

			var publicationWikis = context.WikiPages
				.ThatAreCurrentRevisions()
				.Where(w => w.PageName.StartsWith(LinkConstants.PublicationWikiPage))
				.ToList();
			var systems = context.GameSystems.ToList();
			var systemFrameRates = context.GameSystemFrameRates.ToList();
			var tiers = context.Tiers.ToList();

			foreach (var legacyMovie in legacyMovies)
			{
				try
				{
					string pageName = LinkConstants.PublicationWikiPage + legacyMovie.Id;
					var system = systems.Single(s => s.Id == legacyMovie.SystemId);
					var tier = tiers.Single(t => t.Id == legacyMovie.Id);

					var publication = new Publication
					{
						System = system,
						Tier = tier
					};
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
				
			}

			context.SaveChanges();

			// Set obsoleted by flags
		}

		private static void InsertDummyPublication(int id, string connectionString)
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				using (var cmd = new SqlCommand
				{
					CommandText = $@"
SET IDENTITY_INSERT Publications ON
INSERT INTO Publications
( id, CreateTimeStamp, Frames, GameId, LastUpdateTimeStamp, MovieFile, MovieFileName, RerecordCount, RomId, SubmissionId, SystemFrameRateId, SystemId, TierId)
Values
({id}, getdate(), 1, 1, getdate(), 1, '', 1, 1, 1, 1, 1, 1)",
					Connection = sqlConnection
				})
				{
					sqlConnection.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
