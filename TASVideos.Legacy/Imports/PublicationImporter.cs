using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using Microsoft.EntityFrameworkCore;

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
			// TODO
			// multiple streaming url links
			// multiple archive links
			// multiple movie files
			// multiple torrents

			var publications = new List<Publication>();
			var publicationAuthors = new List<PublicationAuthor>();
			var publicationFiles = new List<PublicationFile>();
			var publicationTags = new List<PublicationTag>();

			using (context.Database.BeginTransaction())
			using (legacySiteContext.Database.BeginTransaction())
			{
				var legacyMovies = legacySiteContext.Movies
					.Include(m => m.MovieFiles)
					.Include(m => m.Publisher)
					.Include(m => m.Player)
					.Where(m => m.Id > 0)
					.ToList();

				var legacyMovieFileStorage = legacySiteContext.MovieFileStorage.ToList();
				var legacyMovieClasses = legacySiteContext.MovieClass.ToList();
				var legacyClassTypes = legacySiteContext.ClassTypes.ToList();

				var legacyUserPlayers = legacySiteContext.UserPlayers.ToList();
				var legacyUsers = legacySiteContext.Users.Select(u => new { u.Id, u.Name }).ToList();

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

				var users = context.Users.ToList();
				var systems = context.GameSystems.ToList();
				var systemFrameRates = context.GameSystemFrameRates.ToList();
				var games = context.Games.ToList();
				var tags = context.Tags.ToList();

				var movieTypes = new[] { "B2", "BK", "C", "6", "2", "S", "B", "L", "W", "3", "Y", "G", "#", "F", "Q", "E", "Z", "X", "U", "I", "R", "8", "4", "9", "7", "F3", "MA" };
				var torrentTypes = new[] { "M", "N", "O", "P", "T" };

				foreach (var legacyMovie in legacyMovies)
				{
					string pageName = LinkConstants.PublicationWikiPage + legacyMovie.Id;
					var wiki = publicationWikis.Single(p => p.PageName == pageName);
					var submission = submissions.Single(s => s.Id == legacyMovie.SubmissionId);

					var system = systems.Single(s => s.Id == legacyMovie.SystemId);
					var systemFrameRate = systemFrameRates.Single(s => s.Id == submission.SystemFrameRateId);

					var game = games.Single(g => g.Id == (submission.GameId ?? -1));

					// Find the first of an acceptable movie type
					var movieFile = legacyMovie.MovieFiles.First(f => movieTypes.Contains(f.Type));

					var screnshotUrl = legacyMovie.MovieFiles.First(f => f.Type == "H");
					var torrentUrls = legacyMovie.MovieFiles.Where(f => torrentTypes.Contains(f.Type));
					var mirror = legacyMovie.MovieFiles.FirstOrDefault(f => f.Type == "A")?.FileName;
					var streaming = (legacyMovie.MovieFiles.FirstOrDefault(f => f.Type == "J" && f.FileName.Contains("youtube"))
						?? legacyMovie.MovieFiles.FirstOrDefault(f => f.Type == "J"))?.FileName;

					var movieFileStorage = legacyMovieFileStorage.Single(lmfs => lmfs.FileName == movieFile.FileName);

					var siteUserIds = legacyUserPlayers
						.Where(p => p.PlayerId == legacyMovie.Player.Id)
						.Select(up => up.UserId)
						.ToList();

					List<string> potentialAuthors;
					if (siteUserIds.Count == 0)
					{
						potentialAuthors = new List<string> { legacyMovie.Player.Name.ToLower() };
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
						CreateUserName = legacyMovie.Publisher.Name ?? "Unknown",
						CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacyMovie.PublishedDate),
						LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacyMovie.PublishedDate), // TODO
						ObsoletedById = legacyMovie.ObsoletedBy == -1 ? null : legacyMovie.ObsoletedBy,
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
						Branch = legacyMovie.Branch,
						MirrorSiteUrl = mirror,
						OnlineWatchingUrl = streaming
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

					publicationFiles.Add(new PublicationFile
					{
						PublicationId = legacyMovie.Id,
						Type = FileType.Screenshot,
						Path = screnshotUrl.FileName,
						CreateTimeStamp = DateTime.UtcNow,
						LastUpdateTimeStamp = DateTime.UtcNow
					});

					publicationFiles.AddRange(torrentUrls.Select(t => new PublicationFile
					{
						PublicationId = legacyMovie.Id,
						Type = FileType.Torrent,
						Path = t.FileName,
						CreateTimeStamp = DateTime.UtcNow,
						LastUpdateTimeStamp = DateTime.UtcNow
					}));

					var mcs = legacyMovieClasses
						.Where(lmc => lmc.MovieId == legacyMovie.Id);

					foreach (var mc in mcs)
					{
						var classType = mc.ClassId >= 1000
							? legacyClassTypes.Single(c => c.Id == mc.ClassId)
							: legacyClassTypes.Single(c => c.OldId == mc.ClassId);

						if (classType.PositiveText.Contains("Genre"))
						{
							continue;
						}

						var tag = mc.Value == 1
							? tags.Single(t => t.DisplayName == classType.PositiveText)
							: tags.Single(t => t.DisplayName == classType.NegativeText);

						publicationTags.Add(new PublicationTag
						{
							PublicationId = legacyMovie.Id,
							TagId = tag.Id
						});
					}
				}
			}

			var pubColumns = new[]
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
				nameof(Publication.Title),
				nameof(Publication.MirrorSiteUrl),
				nameof(Publication.OnlineWatchingUrl),
				nameof(Publication.ObsoletedById)
			};

			publications.BulkInsert(context, pubColumns, nameof(ApplicationDbContext.Publications));

			var pubAuthorColumns = new[]
			{
				nameof(PublicationAuthor.UserId),
				nameof(PublicationAuthor.PublicationId)
			};

			publicationAuthors.BulkInsert(context, pubAuthorColumns, nameof(ApplicationDbContext.PublicationAuthors), SqlBulkCopyOptions.Default);

			var pubFileColumns = new[]
			{
				nameof(PublicationFile.PublicationId),
				nameof(PublicationFile.Path),
				nameof(PublicationFile.Type),
				nameof(PublicationFile.CreateUserName),
				nameof(PublicationFile.LastUpdateUserName),
				nameof(PublicationFile.CreateTimeStamp),
				nameof(PublicationFile.LastUpdateTimeStamp)
			};

			publicationFiles.BulkInsert(context, pubFileColumns, nameof(ApplicationDbContext.PublicationFiles), SqlBulkCopyOptions.Default);

			var pubTagColumns = new[]
			{
				nameof(PublicationTag.PublicationId),
				nameof(PublicationTag.TagId)
			};

			publicationTags.BulkInsert(context, pubTagColumns, nameof(ApplicationDbContext.PublicationTags), SqlBulkCopyOptions.Default);
		}
	}
}
