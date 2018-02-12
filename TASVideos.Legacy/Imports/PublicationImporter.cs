using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Data.Site.Entity;

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

			InsertDummyPublications(legacyMovies, context.Database.GetDbConnection().ConnectionString);
			var newpubs = context.Publications.ToList();

			foreach (var legacyMovie in legacyMovies)
			{
				try
				{
					string pageName = LinkConstants.PublicationWikiPage + legacyMovie.Id;
					var wiki = publicationWikis.Single(p => p.PageName == pageName);
					var system = systems.Single(s => s.Id == legacyMovie.SystemId);
					var tier = tiers.Single(t => t.Id == legacyMovie.Id);

					var publication = newpubs.Single(p => p.Id == legacyMovie.Id);
					publication.System = system;
					publication.Tier = tier;

				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
				
			}

			context.SaveChanges();

			// Set obsoleted by flags
		}

		private static void InsertDummyPublications(IList<Movie> movies, string connectionString)
		{
			var sb = new StringBuilder("SET IDENTITY_INSERT Publications ON ");
			sb.AppendLine(string.Concat(movies.Select(m =>
$@"INSERT INTO Publications ( id, CreateTimeStamp, Frames, GameId, LastUpdateTimeStamp, MovieFile, MovieFileName, RerecordCount, RomId, SubmissionId, SystemFrameRateId, SystemId, TierId)
Values ({m.Id}, getdate(), 1, 1, getdate(), 1, '', 1, 1, 1, 1, 1, 1)
")));

			using (var sqlConnection = new SqlConnection(connectionString))
			{
				using (var cmd = new SqlCommand
				{
					CommandText = sb.ToString(),
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
