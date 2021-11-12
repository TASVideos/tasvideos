using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	internal class PublicationImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext,
			IReadOnlyDictionary<int, int> userIdMapping)
		{
			var publications = new List<Publication>();
			var publicationAuthors = new List<PublicationAuthor>();
			var publicationFiles = new List<PublicationFile>();
			var allUsersWithPlayers = legacySiteContext.Users
				.Include(u => u.UserPlayers)
				.ThenInclude(up => up.Player)
				.ToList();
			var allUsersPlusAdded = context.Users.ToList();

			var legacyMovies = legacySiteContext.Movies
				.Include(m => m.MovieFiles)
				.ThenInclude(mf => mf.Storage)
				.Include(m => m.Publisher)
				.Include(m => m.Player)
				.Where(m => m.Id > 0)
				.ToList();

			var publicationWikis = context.WikiPages
				.ThatAreNotDeleted()
				.WithNoChildren()
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
					s.GameId,
					s.AdditionalAuthors,
					s.System,
					s.SystemFrameRate,
					s.EmulatorVersion
				})
				.ToList();

			var games = context.Games.ToList();

			var movieTypes = new[] { "B2", "BK", "C", "6", "2", "S", "B", "L", "W", "3", "Y", "G", "#", "F", "Q", "E", "Z", "X", "U", "I", "R", "8", "4", "9", "7", "F3", "MA", "LT" };
			var torrentTypes = new[] { "M", "N", "O", "P", "T" };

			var pubs = (from lm in legacyMovies
						join w in publicationWikis on LinkConstants.PublicationWikiPage + lm.Id equals w.PageName
						join s in submissions on lm.SubmissionId equals s.Id
						join g in games on s.GameId ?? -1 equals g.Id
						select new
						{
							Movie = lm,
							Wiki = w,
							Sub = s,
							Game = g
						})
				.ToList();

			foreach (var pub in pubs)
			{
				var movieFiles = pub.Movie.MovieFiles.Where(f => movieTypes.Contains(f.Type)).ToList();
				var mainMovieFile = movieFiles.First(); // Pick the first one to be the official, we have no better way really
				var screenshotUrl = pub.Movie.MovieFiles.First(f => f.Type == "H");
				var torrentUrls = pub.Movie.MovieFiles.Where(f => torrentTypes.Contains(f.Type));

				var publication = new Publication
				{
					Id = pub.Movie.Id,
					WikiContentId = pub.Wiki.Id,
					SubmissionId = pub.Movie.SubmissionId,
					PublicationClassId = pub.Movie.Tier,
					CreateUserName = pub.Movie.Publisher!.Name,
					CreateTimestamp = ImportHelper.UnixTimeStampToDateTime(pub.Movie.PublishedDate),
					LastUpdateTimestamp = ImportHelper.UnixTimeStampToDateTime(pub.Movie.LastChange),
					ObsoletedById = pub.Movie.ObsoletedBy == -1 ? null : pub.Movie.ObsoletedBy,
					Frames = pub.Sub.Frames,
					RerecordCount = pub.Sub.RerecordCount,
					RomId = -1, // Place holder
					GameId = pub.Sub.GameId ?? -1,
					Game = pub.Game,
					MovieFile = mainMovieFile.Storage!.FileData,
					MovieFileName = mainMovieFile.FileName,
					SystemFrameRateId = pub.Sub.SystemFrameRateId ?? 0,
					SystemFrameRate = pub.Sub.SystemFrameRate,
					SystemId = pub.Movie.SystemId,
					System = pub.Sub.System,
					Branch = pub.Movie.Branch.NullIfWhiteSpace(),
					EmulatorVersion = pub.Sub.EmulatorVersion
				};

				var userplayerAuthors = allUsersWithPlayers
					.Where(u => u.UserPlayers.Any(up => up.PlayerId == pub.Movie.Player!.Id) && u.Id != 2355) // user 2355 MICKEY has two accounts, proper one will be added back later
					.ToList();

				var pubTitleAuthorsOriginal = pub.Movie.Player!.Name
					.ParseUserNames()
					.ToList();
				var pubTitleAuthorsConverted = pubTitleAuthorsOriginal
					.Select(a => NickCleanup(a.ToLower()))
					.ToList();

				var accountAuthors = allUsersPlusAdded.Where(u => userplayerAuthors.Any(up => userIdMapping[up.Id] == u.Id)).ToList();
				List<string> additionalAuthors = new ();

				var missingUsers = pubTitleAuthorsConverted.Where(user => userplayerAuthors.All(u => u!.Name.ToLower() != user)).ToList();
				if (missingUsers.Count != 0)
				{
					foreach (var missingUser in missingUsers)
					{
						var foundUser = allUsersPlusAdded.SingleOrDefault(u => u.UserName.ToLower() == missingUser);
						if (foundUser != null)
						{
							accountAuthors.Add(foundUser);
						}
						else
						{
							additionalAuthors.Add(pubTitleAuthorsOriginal[pubTitleAuthorsConverted.IndexOf(missingUser)]);
						}
					}
				}

				if (additionalAuthors.Any())
				{
					publication.AdditionalAuthors = string.Join(",", additionalAuthors);
				}

				var pubAuthors = accountAuthors
					.Select(u => new PublicationAuthor
					{
						UserId = u!.Id,
						Author = u,
						PublicationId = pub.Movie.Id,
						Publication = publication,
						Ordinal = pubTitleAuthorsConverted.IndexOf(u.UserName.ToLower())
					})
					.ToList();

				publicationAuthors.AddRange(pubAuthors);

				foreach (var author in pubAuthors)
				{
					publication.Authors.Add(author);
				}

				publication.GenerateTitle();
				publications.Add(publication);

				publicationFiles.Add(new PublicationFile
				{
					PublicationId = pub.Movie.Id,
					Type = FileType.Screenshot,
					Path = screenshotUrl.FileName,
					CreateTimestamp = DateTime.UtcNow,
					LastUpdateTimestamp = DateTime.UtcNow,
					Description = screenshotUrl.Description.NullIfWhiteSpace(),
					FileData = null
				});

				publicationFiles.AddRange(torrentUrls.Select(t => new PublicationFile
				{
					PublicationId = pub.Movie.Id,
					Type = FileType.Torrent,
					Path = t.FileName,
					CreateTimestamp = DateTime.UtcNow,
					LastUpdateTimestamp = DateTime.UtcNow,
					FileData = null
				}));

				publicationFiles.AddRange(movieFiles.Skip(1).Select(m => new PublicationFile
				{
					PublicationId = pub.Movie.Id,
					Type = FileType.MovieFile,
					Path = m.FileName,
					FileData = m.Storage!.FileData,
					Description = m.FileName.ToLower().Contains("consoleverified")
						? "Console Verication"
						: "Converted to " + Path.GetExtension(m.FileName),
					CreateTimestamp = DateTime.UtcNow,
					LastUpdateTimestamp = DateTime.UtcNow
				}));
			}

			var pubColumns = new[]
			{
				nameof(Publication.Branch),
				nameof(Publication.WikiContentId),
				nameof(Publication.Id),
				nameof(Publication.SubmissionId),
				nameof(Publication.PublicationClassId),
				nameof(Publication.CreateUserName),
				nameof(Publication.CreateTimestamp),
				nameof(Publication.LastUpdateTimestamp),
				nameof(Publication.Frames),
				nameof(Publication.RerecordCount),
				nameof(Publication.GameId),
				nameof(Publication.RomId),
				nameof(Publication.MovieFile),
				nameof(Publication.MovieFileName),
				nameof(Publication.SystemFrameRateId),
				nameof(Publication.SystemId),
				nameof(Publication.Title),
				nameof(Publication.ObsoletedById),
				nameof(Publication.AdditionalAuthors),
				nameof(Publication.EmulatorVersion)
			};

			publications.BulkInsert(pubColumns, nameof(ApplicationDbContext.Publications));

			var pubAuthorColumns = new[]
			{
				nameof(PublicationAuthor.UserId),
				nameof(PublicationAuthor.PublicationId),
				nameof(PublicationAuthor.Ordinal)
			};

			publicationAuthors.BulkInsert(pubAuthorColumns, nameof(ApplicationDbContext.PublicationAuthors));

			var pubFileColumns = new[]
			{
				nameof(PublicationFile.PublicationId),
				nameof(PublicationFile.Path),
				nameof(PublicationFile.Type),
				nameof(PublicationFile.Description),
				nameof(PublicationFile.FileData),
				nameof(PublicationFile.CreateUserName),
				nameof(PublicationFile.LastUpdateUserName),
				nameof(PublicationFile.CreateTimestamp),
				nameof(PublicationFile.LastUpdateTimestamp)
			};

			publicationFiles.BulkInsert(pubFileColumns, nameof(ApplicationDbContext.PublicationFiles));
		}

		// Like in the submission importer, but a different list as publication nicknames can be different from their submission
		private static string NickCleanup(string nickName)
		{
			return nickName switch
			{
				"sjoerdh" => "sjoerd",
				"cherrymay" => "cherry",
				"a.neuhaus" => "alexis_neuhaus",
				"alex_penev" => "alexpenev",
				"bobwhoops" => "bob whoops",
				"ziplock" => "-ziplock-",
				"slash_star_dash" => "/*-",
				"legendofmart" => "mart",
				"michael f" => "michael fried",
				"arnethegreat" => "arne_the_great",
				"samhain-grim" => "vandal",
				"superhappy" => "josh l.",
				"solon" => "bigboct",
				"terrotim" => "trt",
				"ggheysjr" => "ggg",
				"a jesus fan" => "teh noj",
				"snc" => "snc76976",
				"parrot14green" => "parrot14gree",
				"brandon evans" => "brandon",
				"error1" => "errror1",
				"soulcal umbreon" => "soulcal",
				"hƒthor" => "ha›thor",
				"qwerty" => "qwerty6000",
				"k80may" => "heplooner",
				"kien" => "kien_",
				"igorsantos777" => "igoroliveira666",
				"zakky the goatragon" => "zakkydraggy",
				"usta2877" => "bihan",
				"brookman" => "the brookman",
				"mcwave" => "pikachuman",
				"aleckermit" => "alec kermit",
				"akagitsuneyuki" => "akagitsune yukimura",
				"arcree" => "arcree2",
				"the packle" => "thepackle",
				"n?k" => "xxnkxx",
				"david fifield" => "sand",
				"heyhorhey" => "heyhorhey91",
				"lucas wills" => "lucaswills",
				"twistedeye" => "twisted eye",
				"r.bin" => "r-bin2",
				"austin cannon" => "nami",
				"j.y" => "mzscla",
				"le hulk" => "lehulk",
				"moltov" => "moltovm",
				"mickey/vis" => "mickey_vis11189",
				"scaredsim" => "simon sternis",
				"vidar" => "meepers",
				"feeuzz" => "feeuzz22",
				"auxeras" => "th2o",
				"alkdcy" => "alkdc",
				"4232nis" => "nis",
				"01garland01" => "garland",
				"euniversecat" => "euni",
				"zallard1" => "zallard",
				"joka" => "jokaah",
				_ => nickName
			};
		}
	}
}
