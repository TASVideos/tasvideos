using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class SubmissionImporter
	{
		private const double RoundingOffset = 0.005;
		private static readonly string[] ValidSubmissionFileExtensions = { ".dtm", ".mcm", ".gmv", ".dof", ".dsm", ".bkm", ".mcm", ".fm2", ".vbm" };

		// These movies were incorrectly parsed as NTSC, and/or inexplicably did a Math.Ceil instead of rounding so the 60fps detection will fail
		// So we will hard-code these to preserve legacy data
		private static readonly int[] Legacy60FpsOverrides = { 304, 454, 459, 1799, 2571, 2602 };

		// More inexplicably rounding that is avoiding detection as a legacy 50fps movie
		private static readonly int[] Legacy50FpsOverrides = { 766, 1752, 2182 };

		// Incorrectly parsed as Ntsc instead of Pal, but at correct Ntsc fps, not 60
		private static readonly int[] LegacyNtscOverrides = { 2469, 3100, 6353, 4309, 4810 };

		// Need to be PAL but not 50 fps
		private static readonly int[] LegacyPalOverrides = { 5232, 5682, 5754 };

		// These were parsed as PAL but game version does not indicate
		private static readonly int[] C64Pal = { 4526, 5527, 5536, 5543, 5545, 5552, 5554, 5592, 5595, 5596, 5599, 6339 };

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
				.GroupBy(r => r.Id)
				.Select(r => new { Id = r.Key, r.First().Reason })
				.ToList();

			var submissions = new List<Submission>();
			var submissionAuthors = new List<SubmissionAuthor>();
			var submissionHistory = new List<SubmissionStatusHistory>();

			using (context.Database.BeginTransaction())
			{
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
				var systemFrameRates = context.GameSystemFrameRates.ToList();

				var lSubsWithSystem = (from ls in legacySubmissions
					join s in systems on ls.SystemId equals s.Id
					join w in submissionWikis on LinkConstants.SubmissionWikiPage + ls.Id equals w.PageName
					join u in users on ImportHelper.ConvertLatin1String(ls.User.Name).ToLower() equals u.UserName.ToLower() into uu // Some wiki users were never in the forums, and therefore could not be imported (no password for instance)
					from u in uu.DefaultIfEmpty()
					join j in users on ImportHelper.ConvertLatin1String(ls.Judge.Name).ToLower() equals j.UserName.ToLower() into ju
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

					var extension = GetExtension(legacySubmission.Sub.Content);

					GameSystemFrameRate systemFrameRate;

					var movieExtension = GetExtension(legacySubmission.Sub.Content);
					var timeAs50Fps = Math.Round(legacySubmission.Sub.Frames / 50.0, 2);
					var timeAs60Fps = Math.Round(legacySubmission.Sub.Frames / 60.0, 2);
					var legacyTime = Math.Round((double)legacySubmission.Sub.Length, 2);

					if (LegacyNtscOverrides.Contains(legacySubmission.Sub.Id))
					{
						systemFrameRate = systemFrameRates
							.Single(sf => sf.GameSystemId == legacySubmission.System.Id && sf.RegionCode == "NTSC");
					}
					else if (LegacyPalOverrides.Contains(legacySubmission.Sub.Id))
					{
						systemFrameRate = systemFrameRates
							.Single(sf => sf.GameSystemId == legacySubmission.System.Id && sf.RegionCode == "PAL");
					}

					// Legacy support hack. If we have a NTSC60 legacy framerate and the legacy time looks like it was calculated with 60fps
					// Then use this system framerate instead of NTSC
					else if ((Math.Abs(timeAs60Fps - legacyTime) < RoundingOffset
						&& systemFrameRates.Any(sf => sf.GameSystemId == legacySubmission.System.Id && sf.FrameRate == 60))
						|| Legacy60FpsOverrides.Contains(legacySubmission.Sub.Id)
						|| extension == "jrsr") // All these movies were parsed as 60fps, but the database says that DOS is 70fps, weird
					{
						systemFrameRate = systemFrameRates
							.Single(sf => sf.GameSystemId == legacySubmission.System.Id && sf.FrameRate == 60);
					}

					// Legacy support hack. If we have a PAL50 legacy framerate and the legacy time looks like it was calculated with 50fps
					// Then use this system framerate instead of PAL
					else if ((Math.Abs(timeAs50Fps - legacyTime) < RoundingOffset
							&& systemFrameRates.Any(sf => sf.GameSystemId == legacySubmission.System.Id && sf.FrameRate == 50))
						|| Legacy50FpsOverrides.Contains(legacySubmission.Sub.Id))
					{
						systemFrameRate = systemFrameRates
							.Single(sf => sf.GameSystemId == legacySubmission.System.Id && sf.FrameRate == 50);
					}
					else if ((legacySubmission.Sub.GameVersion.ToLower().Contains("euro")
							&& !legacySubmission.Sub.GameVersion.ToLower().Contains("usa")
							&& legacySubmission.System.Id != 10) // SMS Europe games are still 60fps
							|| C64Pal.Contains(legacySubmission.Sub.Id)
							|| legacySubmission.System.Id == 44) // ZX Spectrum is PAL only
					{
						systemFrameRate = systemFrameRates
							.SingleOrDefault(sf => sf.GameSystemId == legacySubmission.System.Id && sf.RegionCode == "PAL")
							?? systemFrameRates.Single(sf => sf.GameSystemId == legacySubmission.System.Id && sf.RegionCode == "NTSC");
					}
					else
					{
						systemFrameRate = systemFrameRates
							.Single(sf => sf.GameSystemId == legacySubmission.System.Id && sf.RegionCode == "NTSC");
					}

					var submission = new Submission
					{
						Id = legacySubmission.Sub.Id,
						WikiContentId = legacySubmission.Wiki.Id,
						SubmitterId = legacySubmission.Submitter?.Id,
						Submitter = legacySubmission.Submitter,
						SystemId = legacySubmission.System.Id,
						System = legacySubmission.System,
						SystemFrameRateId = systemFrameRate.Id,
						SystemFrameRate = systemFrameRate,
						CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacySubmission.Sub.SubmissionDate),
						CreateUserName = legacySubmission.Submitter?.UserName,
						LastUpdateTimeStamp = legacySubmission.Wiki.CreateTimeStamp,
						GameName = ImportHelper.ConvertLatin1String(legacySubmission.Sub.GameName),
						GameVersion = legacySubmission.Sub.GameVersion,
						Frames = legacySubmission.Sub.Frames,
						Status = ConvertStatus(legacySubmission.Sub.Status),
						RomName = legacySubmission.Sub.RomName,
						RerecordCount = legacySubmission.Sub.Rerecord,
						MovieFile = legacySubmission.Sub.Content,
						IntendedTierId = legacySubmission.Sub.IntendedTier,
						GameId = legacySubmission.Sub.GameNameId ?? -1, // Placeholder game if not present
						RomId = -1, // Legacy system had no notion of Rom for submissions
						EmulatorVersion = CleanAndGuessEmuVersion(legacySubmission.Sub.Id, legacySubmission.Sub.EmulatorVersion, extension),
						JudgeId = legacySubmission.Judge?.Id,
						PublisherId = legacySubmission.Publisher?.Id,
						Branch = string.IsNullOrWhiteSpace(legacySubmission.Sub.Branch) ? null : ImportHelper.ConvertLatin1String(legacySubmission.Sub.Branch).Cap(50),
						MovieExtension = movieExtension,
						RejectionReasonId = legacySubmission.Rejection?.Reason
					};

					if (legacySubmission.Sub.Id == 175) // Snow bros, inexplicably JP&JP on submission data
					{
						legacySubmission.Sub.Author = "DJ FozzBozz & RaverMeister";
					}

					var authorNames = legacySubmission.Sub.Author
						.ParseUserNames()
						.Select(a => NickCleanup(ImportHelper.ConvertLatin1String(a).ToLower().Trim()))
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
							SubmissionId = submission.Id,
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
							SubmissionId = submission.Id,
						});
					}

					submission.GenerateTitle();
					submissions.Add(submission);
				}
			}

			var subColumns = new[]
			{
				nameof(Submission.Id),
				nameof(Submission.WikiContentId),
				nameof(Submission.SubmitterId),
				nameof(Submission.SystemId),
				nameof(Submission.SystemFrameRateId),
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
				nameof(Submission.RejectionReasonId)
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
			switch (legacyStatus)
			{
				default:
					throw new NotImplementedException($"unknown status {legacyStatus}");
				case "N":
					return SubmissionStatus.New;
				case "P":
					return SubmissionStatus.PublicationUnderway;
				case "R":
					return SubmissionStatus.Rejected;
				case "K":
					return SubmissionStatus.Accepted;
				case "C":
					return SubmissionStatus.Cancelled;
				case "Q":
					return SubmissionStatus.NeedsMoreInfo;
				case "O":
					return SubmissionStatus.Delayed;
				case "J":
					return SubmissionStatus.JudgingUnderWay;
				case "Y":
					return SubmissionStatus.Published;
			}
		}

		private static SubmissionStatus ConvertJudgeStatus(SubmissionStatus currentStatus)
		{
			switch (currentStatus)
			{
				default:
					throw new NotImplementedException($"Submission Import: Have not consideredunknown status {currentStatus}");
				case SubmissionStatus.New:
					throw new NotImplementedException($"Submission Import: Have not handled scenario: Has judge is in {currentStatus} status");
				case SubmissionStatus.PublicationUnderway:
					return SubmissionStatus.Accepted;
				case SubmissionStatus.Rejected:
					return SubmissionStatus.Rejected;
				case SubmissionStatus.Accepted:
					return SubmissionStatus.Accepted;
				case SubmissionStatus.Cancelled:
					return SubmissionStatus.Cancelled; // Judges cancel submissions on behalf of the author from time to time
				case SubmissionStatus.NeedsMoreInfo:
					throw new NotImplementedException($"Submission Import: Have not handled scenario: Has judge is in {currentStatus} status");
				case SubmissionStatus.Delayed:
					return SubmissionStatus.Delayed;
				case SubmissionStatus.JudgingUnderWay:
					return SubmissionStatus.JudgingUnderWay;
				case SubmissionStatus.Published:
					return SubmissionStatus.Accepted;
			}
		}

		private static string GetExtension(byte[] content)
		{
			if (content == null || content.Length == 0)
			{
				return null;
			}

			using (var submissionFileStream = new MemoryStream(content))
			using (var submissionZipArchive = new ZipArchive(submissionFileStream, ZipArchiveMode.Read))
			{
				var entries = submissionZipArchive.Entries.ToList();
				if (entries.Count != 1)
				{
					// TODO: cleanup multiple entries while we are at it
					return entries
						.Select(e => Path.GetExtension(e.FullName))
						.Where(s => ValidSubmissionFileExtensions.Contains(s))
						.Distinct()
						.FirstOrDefault()
						?.TrimStart('.');
				}

				return Path.GetExtension(entries[0].FullName).TrimStart('.');
			}
		}

		private static string CleanAndGuessEmuVersion(int id, string emulatorVersion, string movieExtension)
		{
			emulatorVersion = string.IsNullOrWhiteSpace(emulatorVersion)
				? null
				: emulatorVersion.Trim();

			if (!string.IsNullOrWhiteSpace(emulatorVersion))
			{
				return emulatorVersion.Cap(50);
			}

			// If still null, guess based on movie extension
			switch (movieExtension)
			{
				default:
					return null;
				case "vbm":
					return "VBA";
				case "fmv":
					return "Famtasia";
				case "gmv":
					return "GENS";
				case "fcm":
					return "FCEU0.98";
				case "m64":
					return "mupen64 0.5 re-recording v8";
				case "smv":
					return id < 1532 // The first known Snes9x 1.51 submission
						? "Snes9x 1.43"
						: "Snes9x";
			}
		}

		// These users have a variation in their nickname vs their actual username, or liked to have different nicknames for who knows why
		private static string NickCleanup(string nickName)
		{
			switch (nickName)
			{
				default:
					return nickName;
				case "slash_star_dash":
				case "slash star dash":
				case "androgony":
					return "/*-";
				case "catlynat":
				case "angelaclaws11s2":
					return "angelaclaws";
				case "iiro2":
					return "anonymous6327";
				case "arne the great":
					return "arne_the_great";
				case "blj":
					return "backwardlongjump";
				case "brookman":
					return "the brookman";
				case "solon":
					return "boct1584";
				case "msteinfield":
					return "aroduc";
				case "lightblueyoshi":
					return "bbkaizo";
				case "bobwhoops":
					return "bob whoops";
				case "brandon evans":
					return "brandon";
				case "sonic tas team":
					return "carretero";
				case "cherrymay":
					return "cherry";
				case "curseschris a.k.a. alen":
					return "curseschris";
				case "david wilson tiziu":
					return "david wilson";
				case "devin":
				case "devin dot com":
					return "devindotcom";
				case "pokedroidtas":
					return "diego montoya";
				case "error1":
					return "errror1";
				case "egxhb":
				case "nidoqueenofpain":
					return "egixbacon";
				case "usta2877":
					return "bihan";
				case "fisker":
				case "fisker n.":
					return "fiskern";
				case "pocoryu":
					return "hellfire";
				case "srb2espyo":
					return "espyo";
				case "mister epic":
					return "gabcm";
				case "gaming-jok":
					return "nico30620";
				case "god hand":
					return "god-hand";
				case "funkdoc":
					return "josh the funkdoc";
				case "hys111111":
					return "gemini-man";
				case "gleisonfodão":
					return "gleison";
				case "ggheysjr":
					return "ggg";
				case "krabe":
					return "bobypoula";
				case "ocean prince":
					return "hegyak";
				case "igorsantos777":
					return "igoroliveira666";
				case "lag.com":
					return "lagdotcom";
				case "toonlucas22":
					return "limo";
				case "p.dot":
					return "dashiznawz";
				case "undo":
					return "jonathangm";
				case "superhappy":
					return "josh l.";
				case "legendofmart":
					return "mart";
				case "mat1er/sblurb":
					return "mat1er";
				case "mickey/vis":
					return "mickey_vis11189";
				case "madhatter":
					return "mr. kelly r. flewin";
				case "superninja":
					return "luke";
				case "mazzic kiegel":
					return "mazzic";
				case "michael f":
				case "エジソン電":
					return "michael fried";
				case "tehseven":
					return "negative seven";
				case "foda":
					return "nesrocks";
				case "mr_eeh":
					return "mbm";
				case "david z":
					return "mclaud2000";
				case "dmtm":
				case "laecktoer":
					return "mr_sweed";
				case "nif_boy":
					return "nifboy";
				case "aka":
					return "mitjitsu";
				case "程嘉军":
				case "j.y":
					return "mzscla";
				case "parrot14green":
					return "parrot14gree";
				case "p3run4":
					return "p3r";
				case "promise":
					return "ouendan";
				case "nicfer":
					return "perfect death";
				case "jok-r":
					return "nico30620";
				case "phil.":
					return "phil";
				case "mcw4v3-x":
					return "pikachuman";
				case "shakespeare":
					return "radz";
				case "snc":
					return "snc76976";
				case "mattias b.":
					return "tarzan";
				case "m-eighty":
					return "mike89";
				case "jossepi":
					return "totoro";
				case "symbolic x":
					return "p0rtal_0f_rain";
				case "primo":
					return "primorial#soup";
				case "saegotomin":
					return "megaman";
				case "romaji":
					return "sack_bot";
				case "toxicparrot":
					return "sam";
				case "soulcal umbreon":
					return "soulcal";
				case "superflorian12":
					return "supermario12";
				case "terrotim":
					return "trt";
				case "samhain-grim":
					return "vandal";
				case "a jesus fan":
					return "teh noj";
				case "walkerboh":
					return "walker boh";
				case "西坡":
				case "西坡 (xipo)":
					return "xipo";
				case "n?k":
					return "xxnkxx";
				case "mechakoopa_dttvb":
				case "mechakoopa revolution":
					return "yagz";
				case "lorenzo_the_comic":
					return "yoni arousement";
				case "lazy_zefiris":
					return "zefiris";
				case "ziplock":
					return "-ziplock-";
				case "zakky the goatragon":
					return "zakkydraggy";
				case "the packle":
					return "thepackle";
				case "kowka":
					return "goddessmaria";
				case "lars":
					return "lars_hendrick";
				case "lazerlemongaming":
					return "lazerlemon";
				case "akagitsuneyuki":
					return "akagitsune yukimura";
				case "dekutary":
					return "dekutony";
				case "wasapi_fi":
					return "wasapi";
				case "daniel_1rd":
					return "dbxv";
				case "no0bplayer":
					return "playerone";
				case "mario3264":
					return "mario128";
				case "01garland01":
					return "garland";
				case "tapioca2k":
					return "tapioca";
				case "smbbot":
					return "happylee";
				case "fusionvaria":
					return "itspersonnal";
				case "duault":
					return "zekann";
			}
		}
	}
}
