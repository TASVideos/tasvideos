using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using FastMember;

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
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.Where(w => w.PageName.StartsWith(LinkConstants.PublicationWikiPage))
				.Select(s => new { s.Id, s.PageName })
				.ToList();

			var submissions = context.Submissions
				.Select(s => new
				{
					s.Id,
					s.SystemFrameRateId,
					s.Frames,
					s.RerecordCount
				})
				.ToList();

			List<Publication> publications = new List<Publication>();

			foreach (var legacyMovie in legacyMovies)
			{
				try
				{
					string pageName = LinkConstants.PublicationWikiPage + legacyMovie.Id;
					var wiki = publicationWikis.Single(p => p.PageName == pageName);
					var submission = submissions.Single(s => s.Id == legacyMovie.SubmissionId);
					

					var publication = new Publication
					{
						Id = legacyMovie.Id,
						SubmissionId = legacyMovie.SubmissionId,
						TierId = legacyMovie.Tier,
						//CreateUserName = // TODO: publisher?,
						CreateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyMovie.PublishedDate),
						LastUpdateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyMovie.PublishedDate), // TODO
						ObsoletedById = legacyMovie.ObsoletedBy,
						Frames = submission.Frames,
						RerecordCount = submission.RerecordCount
					};

					publications.Add(publication);

				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}

			var copyParams = new[]
			{
				nameof(Publication.Id),
				nameof(Publication.SubmissionId),
				nameof(Publication.TierId),
				nameof(Publication.CreateUserName),
				nameof(Publication.CreateTimeStamp),
				nameof(Publication.LastUpdateTimeStamp),
				nameof(Publication.Frames),
				nameof(Publication.RerecordCount)
			};

			using (var sqlCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString, SqlBulkCopyOptions.KeepIdentity))
			{
				sqlCopy.DestinationTableName = $"[{nameof(ApplicationDbContext.Publications)}]";
				sqlCopy.BatchSize = 10000;

				foreach (var param in copyParams)
				{
					sqlCopy.ColumnMappings.Add(param, param);
				}

				using (var reader = ObjectReader.Create(publications, copyParams))
				{
					sqlCopy.WriteToServer(reader);
				}
			}
		}
	}
}
