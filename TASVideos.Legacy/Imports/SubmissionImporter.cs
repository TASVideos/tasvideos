using System;
using System.Collections.Generic;
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
		public static void Import(
			string connectionStr,
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			// TODO:
			// submitters not in forum 
			// MovieExtension
			var legacySubmissions = legacySiteContext.Submissions
				.Include(s => s.User)
				.Include(s => s.Judge)
				.Include(s => s.Movie)
				.Where(s => s.Id > 0)
				.ToList();

			var submissions = new List<Submission>();
			var submissionAuthors = new List<SubmissionAuthor>();
			var submissionHistory = new List<SubmissionStatusHistory>();

			using (context.Database.BeginTransaction())
			{
				var users = context.Users.ToList();

				var submissionWikis = context.WikiPages
					.ThatAreNotDeleted()
					.ThatAreCurrentRevisions()
					.Where(w => w.PageName.StartsWith(LinkConstants.SubmissionWikiPage))
					.Select(s => new { s.Id, s.PageName })
					.ToList();

				var publicationWikis = context.WikiPages
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
					select new { Sub = ls, System = s, Wiki = w, Submitter = u, Judge = j, Publisher = p, PubDate = pub?.CreateTimeStamp })
					.ToList();

				foreach (var legacySubmission in lSubsWithSystem)
				{
					GameSystemFrameRate systemFrameRate;

					if (legacySubmission.Sub.GameVersion.ToLower().Contains("euro")
						|| legacySubmission.System.Id == 44) // ZX Spectrum which has no NTSC
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
						LastUpdateTimeStamp = DateTime.UtcNow, // TODO
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
						EmulatorVersion = legacySubmission.Sub.EmulatorVersion?.Cap(50),
						JudgeId = legacySubmission.Judge?.Id,
						PublisherId = legacySubmission.Publisher?.Id ?? null,
						Branch = string.IsNullOrWhiteSpace(legacySubmission.Sub.Branch) ? null : ImportHelper.ConvertLatin1String(legacySubmission.Sub.Branch).Cap(50)
					};

					var authorNames = legacySubmission.Sub.Author
						.ParseUserNames()
						.Select(a => a.ToLower())
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
				nameof(Submission.PublisherId)
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
	}
}
