using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class SubmissionImporter
	{
		private static readonly string[] ValidSubmissionFileExtensions = { "fmv", "vmv", "fcm", "smv", "dtm", "mcm", "gmv", "dof", "dsm", "bkm", "mcm", "fm2", "vbm", "m64", "mmv", "zmv", "pxm", "fbm", "mc2", "ymv", "jrsr", "gz", "omr", "pjm", "wtf", "tas", "lsmv", "fm3", "bk2", "lmp", "mcm", "mar", "ltm" };

		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			// TODO:
			// submitters not in forum 
			// Cleanup archives by removing multiple entries and other junk
			var legacySubmissions = legacySiteContext.Submissions
				.Include(s => s.User)
				.Include(s => s.Judge)
				.Include(s => s.Movie)
				.Where(s => s.Id > 0)
				.ToList();

			var rejectionReasons = legacySiteContext.SubmissionRejections
				.ToList() // TODO: optimize me, EF 3 doesn't like .First() in there
				.GroupBy(r => r.Id)
				.Select(r => new { Id = r.Key, r.First().Reason })
				.ToList();

			var submissions = new List<Submission>();
			var submissionAuthors = new List<SubmissionAuthor>();
			var submissionHistory = new List<SubmissionStatusHistory>();

			var users = context.Users.ToList();

			var submissionWikis = context.WikiPages
				.ThatAreNotDeleted()
				.WithNoChildren()
				.Where(w => w.PageName.StartsWith(LinkConstants.SubmissionWikiPage))
				.Select(s => new { s.Id, s.PageName, s.CreateTimeStamp })
				.ToList();

			var publicationWikis = context.WikiPages
				.ThatAreNotDeleted()
				.WithNoChildren()
				.Where(w => w.PageName.StartsWith(LinkConstants.PublicationWikiPage))
				.ToList()
				.GroupBy(gkey => gkey.PageName, gvalue => new { gvalue.CreateTimeStamp, gvalue.CreateUserName, gvalue.PageName })
				.Select(p => p.First(g => g.CreateTimeStamp == p.Min(pp => pp.CreateTimeStamp)))
				.ToList();

			var systems = context.GameSystems.ToList();

			var lSubsWithSystem = (from ls in legacySubmissions
				join s in systems on ls.SystemId equals s.Id
				join w in submissionWikis on LinkConstants.SubmissionWikiPage + ls.Id equals w.PageName
				join u in users on ImportHelper.ConvertNotNullLatin1String(ls.User!.Name).ToLower() equals u.UserName.ToLower() into uu // Some wiki users were never in the forums, and therefore could not be imported (no password for instance)
				from u in uu.DefaultIfEmpty()
				join j in users on ImportHelper.ConvertNotNullLatin1String(ls.Judge!.Name).ToLower() equals j.UserName.ToLower() into ju
				from j in ju.DefaultIfEmpty()
				join pub in publicationWikis on LinkConstants.PublicationWikiPage + (ls.Movie?.Id ?? -1) equals pub.PageName into pubs
				from pub in pubs.DefaultIfEmpty()
				join p in users on ImportHelper.ConvertLatin1String(pub?.CreateUserName) equals p.UserName into pp
				from p in pp.DefaultIfEmpty()
				join r in rejectionReasons on ls.Id equals r.Id into rr
				from r in rr.DefaultIfEmpty()
				select new { Sub = ls, System = s, Wiki = w, Submitter = u, Judge = j, Publisher = p, PubDate = pub?.CreateTimeStamp, Rejection = r })
				.ToList();

			foreach (var legacySubmission in lSubsWithSystem)
			{
				if (legacySubmission.Sub.GameVersion == "PAL")
				{
					legacySubmission.Sub.GameVersion = "Europe";
				}

				var (movieExtension, fileData) = CleanupZip(legacySubmission.Sub.Content);
				var legacyTime = Math.Round((double)legacySubmission.Sub.Length, 2);

				var submission = new Submission
				{
					Id = legacySubmission.Sub.Id,
					WikiContentId = legacySubmission.Wiki.Id,
					SubmitterId = legacySubmission.Submitter?.Id,
					Submitter = legacySubmission.Submitter,
					SystemId = legacySubmission.System.Id,
					System = legacySubmission.System,
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacySubmission.Sub.SubmissionDate),
					CreateUserName = legacySubmission.Submitter?.UserName,
					LastUpdateTimeStamp = legacySubmission.Wiki.CreateTimeStamp,
					GameName = ImportHelper.ConvertLatin1String(legacySubmission.Sub.GameName),
					GameVersion = legacySubmission.Sub.GameVersion,
					Frames = legacySubmission.Sub.Frames,
					Status = ConvertStatus(legacySubmission.Sub.Status),
					RomName = legacySubmission.Sub.RomName,
					RerecordCount = legacySubmission.Sub.Rerecord,
					MovieFile = fileData,
					IntendedTierId = legacySubmission.Sub.IntendedTier,
					GameId = legacySubmission.Sub.GameNameId ?? -1, // Placeholder game if not present
					RomId = -1, // Legacy system had no notion of Rom for submissions
					EmulatorVersion = CleanAndGuessEmuVersion(legacySubmission.Sub.Id, legacySubmission.Sub.EmulatorVersion, movieExtension),
					JudgeId = legacySubmission.Judge?.Id,
					PublisherId = legacySubmission.Publisher?.Id,
					Branch = string.IsNullOrWhiteSpace(legacySubmission.Sub.Branch) ? null : ImportHelper.ConvertLatin1String(legacySubmission.Sub.Branch).Cap(50),
					MovieExtension = movieExtension,
					RejectionReasonId = legacySubmission.Rejection?.Reason,
					MovieStartType = ParseAlertsToStartType(legacySubmission.Sub.Alerts),

				};

				submission.LegacyTime = legacySubmission.Sub.Length;
				submission.ImportedTime = 0.0M; // decimal.Round((decimal)(submission.Frames / submission.SystemFrameRate.FrameRate), 3);
				submission.LegacyAlerts = ImportHelper.NullIfWhiteSpace(ImportHelper.ConvertLatin1String(legacySubmission.Sub.Alerts));

				if (legacySubmission.Sub.Id == 175) // Snow bros, inexplicably JP&JP on submission data
				{
					legacySubmission.Sub.Author = "DJ FozzBozz & RaverMeister";
				}

				var authorNames = legacySubmission.Sub.Author
					.ParseUserNames()
					.Select(a => NickCleanup(ImportHelper.ConvertNotNullLatin1String(a).ToLower().Trim()))
					.ToList();

				var authors = users
					.Where(u => authorNames.Contains(u.UserName.ToLower()))
					.Select(u => new SubmissionAuthor
					{
						SubmissionId = submission.Id,
						Submission = submission,
						UserId = u.Id,
						Author = u
					})
					.ToList();

				var additionalAuthors = authorNames.Except(authors.Select(a => a.Author!.UserName.ToLower())).ToList();
				if (additionalAuthors.Any())
				{
					submission.AdditionalAuthors = string.Join(",", additionalAuthors);
				}

				foreach (var author in authors)
				{
					submission.SubmissionAuthors.Add(author);
					submissionAuthors.Add(author);
				}

				if (legacySubmission.Judge != null)
				{
					submissionHistory.Add(new SubmissionStatusHistory
					{
						CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacySubmission.Sub.JudgeDate),
						CreateUserName = legacySubmission.Judge.UserName,
						LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacySubmission.Sub.JudgeDate),
						LastUpdateUserName = legacySubmission.Judge.UserName,
						Status = ConvertJudgeStatus(submission.Status),
						SubmissionId = submission.Id
					});
				}

				if (legacySubmission.Publisher != null && legacySubmission.PubDate.HasValue)
				{
					submissionHistory.Add(new SubmissionStatusHistory
					{
						CreateTimeStamp = legacySubmission.PubDate.Value,
						CreateUserName = legacySubmission.Publisher.UserName,
						LastUpdateTimeStamp = legacySubmission.PubDate.Value,
						LastUpdateUserName = legacySubmission.Publisher.UserName,
						Status = SubmissionStatus.Published,
						SubmissionId = submission.Id
					});
				}

				submission.Title = "";
				submissions.Add(submission);
			}

			var subColumns = new[]
			{
				nameof(Submission.Id),
				nameof(Submission.WikiContentId),
				nameof(Submission.SubmitterId),
				nameof(Submission.SystemId),
				nameof(Submission.CreateTimeStamp),
				nameof(Submission.CreateUserName),
				nameof(Submission.LastUpdateTimeStamp),
				nameof(Submission.GameName),
				nameof(Submission.GameVersion),
				nameof(Submission.Frames),
				nameof(Submission.Status),
				nameof(Submission.RomName),
				nameof(Submission.RerecordCount),
				nameof(Submission.MovieFile),
				nameof(Submission.IntendedTierId),
				nameof(Submission.Title),
				nameof(Submission.GameId),
				nameof(Submission.RomId),
				nameof(Submission.EmulatorVersion),
				nameof(Submission.JudgeId),
				nameof(Submission.Branch),
				nameof(Submission.PublisherId),
				nameof(Submission.MovieExtension),
				nameof(Submission.RejectionReasonId),
				nameof(Submission.AdditionalAuthors),
				nameof(Submission.MovieStartType),
				nameof(Submission.LegacyTime),
				nameof(Submission.ImportedTime),
				nameof(Submission.LegacyAlerts)
			};

			submissions.BulkInsert(connectionStr, subColumns, nameof(ApplicationDbContext.Submissions), bulkCopyTimeout: 600);

			var subAuthorColumns = new[]
			{
				nameof(SubmissionAuthor.UserId),
				nameof(SubmissionAuthor.SubmissionId)
			};

			submissionAuthors.BulkInsert(connectionStr, subAuthorColumns, nameof(ApplicationDbContext.SubmissionAuthors));

			var statusHistoryColumns = new[]
			{
				nameof(SubmissionStatusHistory.CreateTimeStamp),
				nameof(SubmissionStatusHistory.CreateUserName),
				nameof(SubmissionStatusHistory.LastUpdateTimeStamp),
				nameof(SubmissionStatusHistory.LastUpdateUserName),
				nameof(SubmissionStatusHistory.Status),
				nameof(SubmissionStatusHistory.SubmissionId)
			};

			submissionHistory.BulkInsert(connectionStr, statusHistoryColumns, nameof(ApplicationDbContext.SubmissionStatusHistory));
		}

		private static SubmissionStatus ConvertStatus(string legacyStatus)
		{
			return legacyStatus switch
			{
				"N" => SubmissionStatus.New,
				"P" => SubmissionStatus.Accepted, // TODO: should be SubmissionStatus.PublicationUnderway, but then publishers have no way to handle the submission due to the legacy site not tracking the publisher, this allows for ease of testing
				"R" => SubmissionStatus.Rejected,
				"K" => SubmissionStatus.Accepted,
				"C" => SubmissionStatus.Cancelled,
				"Q" => SubmissionStatus.NeedsMoreInfo,
				"O" => SubmissionStatus.Delayed,
				"J" => SubmissionStatus.JudgingUnderWay,
				"Y" => SubmissionStatus.Published,
				_ => throw new NotImplementedException($"unknown status {legacyStatus}")
			};
		}

		private static SubmissionStatus ConvertJudgeStatus(SubmissionStatus currentStatus)
		{
			return currentStatus switch
			{
				SubmissionStatus.New => throw new NotImplementedException($"Submission Import: Have not handled scenario: Has judge is in {currentStatus} status"),
				SubmissionStatus.PublicationUnderway => SubmissionStatus.Accepted,
				SubmissionStatus.Rejected => SubmissionStatus.Rejected,
				SubmissionStatus.Accepted => SubmissionStatus.Accepted,
				SubmissionStatus.Cancelled => SubmissionStatus.Cancelled, // Judges cancel submissions on behalf of the author from time to time
				SubmissionStatus.NeedsMoreInfo => throw new NotImplementedException($"Submission Import: Have not handled scenario: Has judge is in {currentStatus} status"),
				SubmissionStatus.Delayed => SubmissionStatus.Delayed,
				SubmissionStatus.JudgingUnderWay => SubmissionStatus.JudgingUnderWay,
				SubmissionStatus.Published => SubmissionStatus.Accepted,
				_ => throw new NotImplementedException($"Submission Import: Have not considered unknown status {currentStatus}")
			};
		}

		private static (string?, byte[]) CleanupZip(byte[]? content)
		{
			try
			{
				if (content == null || content.Length == 0)
				{
					return (null, new byte[0]);
				}

				using var submissionFileStream = new MemoryStream(content);
				using var submissionZipArchive = new ZipArchive(submissionFileStream, ZipArchiveMode.Read);
				var entries = submissionZipArchive.Entries.ToList();

				var single = entries
					.Where(e => !string.IsNullOrWhiteSpace(Path.GetExtension(e.FullName)))
					.Where(e => (ValidSubmissionFileExtensions).Contains(Path.GetExtension(e.FullName).Replace(".", "")))
					.Distinct()
					.OrderBy(e => e.FullName.Contains("/"))
					.ThenBy(e => e.FullName)
					.First();

				using var singleStream = new MemoryStream();
				using var stream = single.Open();
				stream.CopyTo(singleStream);
				var fileBytes = singleStream.ToArray();
				
				byte[] compressedBytes;
				using (var outStream = new MemoryStream())
				{
					using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
					{
						var fileInArchive = archive.CreateEntry(single.FullName, CompressionLevel.Optimal);
						using var entryStream = fileInArchive.Open();
						using var fileToCompressStream = new MemoryStream(fileBytes);
						fileToCompressStream.CopyTo(entryStream);
					}
					compressedBytes = outStream.ToArray();
				}

				var ext = Path.GetExtension(single.FullName).TrimStart('.');
				return (ext, compressedBytes);
			}
			catch (Exception)
			{
				// Some submissions are .bz2 4551, 6551
				// Some have corrupt headers, 5111
				return (null, content ?? new byte[0]);
			}
		}

		private static string? CleanAndGuessEmuVersion(int id, string? emulatorVersion, string? movieExtension)
		{
			emulatorVersion = string.IsNullOrWhiteSpace(emulatorVersion)
				? null
				: emulatorVersion.Trim();

			if (!string.IsNullOrWhiteSpace(emulatorVersion))
			{
				return emulatorVersion.Cap(50);
			}

			// If still null, guess based on movie extension
			return movieExtension switch
			{
				"vbm" => "VBA",
				"fmv" => "Famtasia",
				"gmv" => "GENS",
				"fcm" => "FCEU0.98",
				"m64" => "mupen64 0.5 re-recording v8",
				"smv" => (id < 1532 // The first known Snes9x 1.51 submission
					? "Snes9x 1.43"
					: "Snes9x"),
				_ => null
			};
		}

		private static int? ParseAlertsToStartType(string? alerts)
		{
			if (!string.IsNullOrWhiteSpace(alerts))
			{
				if (alerts.ToLower().Contains("from dirty SRAM")
				|| alerts.ToLower().Contains("from SRAM"))
				{
					return 1;
				}

				if (alerts.ToLower().Contains("preinitialized memory card"))
				{
					return 1;
				}

				if (alerts.ToLower().Contains("from savestate")
					|| alerts.ToLower().Contains("from a savestate")
					|| alerts.ToLower().Contains("begins from a snapshot"))
				{
					return 2;
				}

				if (alerts.ToLower().Contains("M64 file does not being from power-on"))
				{
					return 2; // Must be savestate, Mupen does not support SRAM backed movies
				}
			}

			return null;
		}

		// These users have a variation in their nickname vs their actual username, or liked to have different nicknames for who knows why
		private static string NickCleanup(string nickName)
		{
			return nickName switch
			{
				"slash_star_dash" => "/*-",
				"slash star dash" => "/*-",
				"androgony" => "/*-",
				"anty-lemon" => "antymew",
				"arukado's" => "arukado",
				"catlynat" => "angelaclaws",
				"angelaclaws11s2" => "angelaclaws",
				"iiro2" => "anonymous6327",
				"arne the great" => "arne_the_great",
				"blj" => "backwardlongjump",
				"bagofmagicfood" => "bag of magic food",
				"brookman" => "the brookman",
				"solon" => "boct1584",
				"msteinfield" => "aroduc",
				"lightblueyoshi" => "bbkaizo",
				"bobwhoops" => "bob whoops",
				"brandon evans" => "brandon",
				"sonic tas team" => "carretero",
				"cherrymay" => "cherry",
				"curseschris a.k.a. alen" => "curseschris",
				"david wilson tiziu" => "david wilson",
				"devin" => "devindotcom",
				"devin dot com" => "devindotcom",
				"pokedroidtas" => "diego montoya",
				"error1" => "errror1",
				"egxhb" => "egixbacon",
				"nidoqueenofpain" => "egixbacon",
				"usta2877" => "bihan",
				"fisker" => "fiskern",
				"fisker n." => "fiskern",
				"pocoryu" => "hellfire",
				"srb2espyo" => "espyo",
				"mister epic" => "gabcm",
				"gaming-jok" => "nico30620",
				"god hand" => "god-hand",
				"funkdoc" => "josh the funkdoc",
				"hys111111" => "gemini-man",
				"gleisonfodão" => "gleison",
				"ggheysjr" => "ggg",
				"krabe" => "bobypoula",
				"ocean prince" => "hegyak",
				"igorsantos777" => "igoroliveira666",
				"lag.com" => "lagdotcom",
				"vidar" => "meepers",
				"toonlucas22" => "limo",
				"p.dot" => "dashiznawz",
				"undo" => "jonathangm",
				"superhappy" => "josh l.",
				"legendofmart" => "mart",
				"mat1er/sblurb" => "mat1er",
				"mickey/vis" => "mickey_vis11189",
				"madhatter" => "mr. kelly r. flewin",
				"superninja" => "luke",
				"mazzic kiegel" => "mazzic",
				"michael f" => "michael fried",
				"エジソン電" => "michael fried",
				"tehseven" => "negative seven",
				"foda" => "nesrocks",
				"mr_eeh" => "mbm",
				"david z" => "mclaud2000",
				"dmtm" => "mr_sweed",
				"laecktoer" => "mr_sweed",
				"nif_boy" => "nifboy",
				"aka" => "mitjitsu",
				"程嘉军" => "mzscla",
				"j.y" => "mzscla",
				"parrot14green" => "parrot14gree",
				"p3run4" => "p3r",
				"promise" => "ouendan",
				"nicfer" => "perfect death",
				"jok-r" => "nico30620",
				"phil." => "phil",
				"phil. côté" => "phil",
				"mcw4v3-x" => "pikachuman",
				"shakespeare" => "radz",
				"snc" => "snc76976",
				"mattias b." => "tarzan",
				"m-eighty" => "mike89",
				"jossepi" => "totoro",
				"symbolic x" => "p0rtal_0f_rain",
				"primo" => "primorial#soup",
				"saegotomin" => "megaman",
				"romaji" => "sack_bot",
				"toxicparrot" => "sam",
				"soulcal umbreon" => "soulcal",
				"superflorian12" => "supermario12",
				"terrotim" => "trt",
				"samhain-grim" => "vandal",
				"samlaptop" => "samtastic",
				"a jesus fan" => "teh noj",
				"walkerboh" => "walker boh",
				"西坡" => "xipo",
				"西坡 (xipo)" => "xipo",
				"n?k" => "xxnkxx",
				"mechakoopa_dttvb" => "yagz",
				"mechakoopa revolution" => "yagz",
				"lorenzo_the_comic" => "yoni arousement",
				"lazy_zefiris" => "zefiris",
				"ziplock" => "-ziplock-",
				"zakky the goatragon" => "zakkydraggy",
				"the packle" => "thepackle",
				"kowka" => "goddessmaria",
				"lars" => "lars_hendrick",
				"lazerlemongaming" => "lazerlemon",
				"akagitsuneyuki" => "akagitsune yukimura",
				"dekutary" => "dekutony",
				"wasapi_fi" => "wasapi",
				"daniel_1rd" => "dbxv",
				"no0bplayer" => "playerone",
				"mario3264" => "mario128",
				"01garland01" => "garland",
				"tapioca2k" => "tapioca",
				"smbbot" => "happylee",
				"fusionvaria" => "itspersonnal",
				"duault" => "zekann",
				_ => nickName
			};
		}
	}
}
