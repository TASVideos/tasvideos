using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using FastMember;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
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

			var legacyWikiUsers = legacySiteContext.Users.Select(u => new { u.Id, u.Name }).ToList();
			var legacyUserPlayers = legacySiteContext.UserPlayers.ToList();
			var legacyUsers = legacySiteContext.Users.ToList();

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
					s.RerecordCount,
					s.GameId
				})
				.ToList();

			var publications = new List<Publication>();
			var publicationAuthors = new List<PublicationAuthor>();
			var users = context.Users.ToList();
			var players = legacySiteContext.Players.ToList();
			var systems = context.GameSystems.ToList();
			var systemFrameRates = context.GameSystemFrameRates.ToList();
			var games = context.Games.ToList();
			

			foreach (var legacyMovie in legacyMovies)
			{
				string pageName = LinkConstants.PublicationWikiPage + legacyMovie.Id;
				var wiki = publicationWikis.Single(p => p.PageName == pageName);
				var submission = submissions.Single(s => s.Id == legacyMovie.SubmissionId);
				var files = legacyMovieFiles
					.Where(lmf => lmf.MovieId == legacyMovie.Id)
					.ToList();
				var system = systems.Single(s => s.Id == legacyMovie.SystemId);
				var publisher = legacyWikiUsers.Single(u => u.Id == legacyMovie.PublisherId);
				var systemFrameRate = systemFrameRates.Single(s => s.Id == submission.SystemFrameRateId);

				var game = games.Single(g => g.Id == (submission.GameId ?? -1));

				// Find the first of an acceptable movie type
				var movieTypes = new[] { "B2", "BK", "C", "6", "2", "S", "B", "L", "W", "3", "Y", "G", "#", "F", "Q", "E", "Z", "X", "U", "I", "R", "8", "4", "9", "7", "F3", "MA" };
				var movieFile = files.First(f => movieTypes.Contains(f.Type));

				var player = players.Single(p => p.Id == legacyMovie.PlayerId);

				var movieFileStorage = legacyMovieFileStorage.Single(lmfs => lmfs.FileName == movieFile.FileName);

				var siteUserIds = legacyUserPlayers
					.Where(p => p.PlayerId == player.Id)
					.Select(up => up.UserId)
					.ToList();

				List<string> potentialAuthors;
				if (siteUserIds.Count == 0)
				{
					potentialAuthors = new List<string> { player.Name.ToLower() };
				}
				else
				{
					potentialAuthors = legacyUsers
					.Where(u => siteUserIds.Contains(u.Id))
					.Select(u => u.Name.ToLower())
					.ToList();
				}

				var publication = new Publication
				{
					Id = legacyMovie.Id,
					WikiContentId = wiki.Id,
					SubmissionId = legacyMovie.SubmissionId,
					TierId = legacyMovie.Tier,
					CreateUserName = publisher.Name ?? "Unknown",
					CreateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyMovie.PublishedDate),
					LastUpdateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyMovie.PublishedDate), // TODO
					ObsoletedById = legacyMovie.ObsoletedBy,
					Frames = submission.Frames,
					RerecordCount = submission.RerecordCount,
					RomId = -1, // Place holder
					GameId = submission.GameId ?? -1,
					Game = game,
					MovieFile = movieFileStorage.FileData,
					MovieFileName = movieFile.FileName,
					SystemFrameRateId = submission.SystemFrameRateId.Value,
					SystemFrameRate = systemFrameRate,
					SystemId = legacyMovie.SystemId,
					System = system,
					Branch = legacyMovie.Branch
				};

				var pauthors = users
					.Where(u => potentialAuthors.Contains(u.UserName.ToLower()))
					.Select(u => new PublicationAuthor
					{
						UserId = u.Id,
						Author = u,
						PublicationId = legacyMovie.Id,
						Pubmisison = publication,
					})
					.ToList();

				publicationAuthors.AddRange(pauthors);

				foreach (var author in pauthors)
				{
					publication.Authors.Add(author);
				}
				
				publication.GenerateTitle();
				publications.Add(publication);
			}

			var copyParams = new[]
			{
				nameof(Publication.Branch),
				nameof(Publication.WikiContentId),
				nameof(Publication.Id),
				nameof(Publication.SubmissionId),
				nameof(Publication.TierId),
				nameof(Publication.CreateUserName),
				nameof(Publication.CreateTimeStamp),
				nameof(Publication.LastUpdateTimeStamp),
				nameof(Publication.Frames),
				nameof(Publication.RerecordCount),
				nameof(Publication.GameId),
				nameof(Publication.RomId),
				nameof(Publication.MovieFile),
				nameof(Publication.MovieFileName),
				nameof(Publication.SystemFrameRateId),
				nameof(Publication.SystemId),
				nameof(Publication.Title)
			};

			var authorParams = new[]
			{
				nameof(PublicationAuthor.UserId),
				nameof(PublicationAuthor.PublicationId)
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

			using (var authorSqlCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString))
			{
				authorSqlCopy.DestinationTableName = $"[{nameof(ApplicationDbContext.PublicationAuthors)}]";
				authorSqlCopy.BatchSize = 10000;

				foreach (var param in authorParams)
				{
					authorSqlCopy.ColumnMappings.Add(param, param);
				}

				using (var reader = ObjectReader.Create(publicationAuthors, authorParams))
				{
					authorSqlCopy.WriteToServer(reader);
				}
			}
		}
	}
}
