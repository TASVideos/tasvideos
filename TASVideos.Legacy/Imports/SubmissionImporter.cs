using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
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
			// Judge (if StatusBy and Status or judged_by
			// Publisher (if StatusBy and Status
			// authors that are not submitters
			// submitters not in forum 
			// MovieExtension
			var legacySubmissions = legacySiteContext.Submissions
				.Include(s => s.User)
				.Where(s => s.Id > 0)
				.ToList();

			var submissions = new List<Submission>();
			var submissionAuthors = new List<SubmissionAuthor>();

			using (context.Database.BeginTransaction())
			{
				var users = context.Users.ToList();

				var submissionWikis = context.WikiPages
					.ThatAreNotDeleted()
					.ThatAreCurrentRevisions()
					.Where(w => w.PageName.StartsWith(LinkConstants.SubmissionWikiPage))
					.Select(s => new { s.Id, s.PageName })
					.ToList();

				var systems = context.GameSystems.ToList();
				var systemFrameRates = context.GameSystemFrameRates.ToList();

				var lSubsWithSystem = (from ls in legacySubmissions
					join s in systems on ls.SystemId equals s.Id
					join w in submissionWikis on LinkConstants.SubmissionWikiPage + ls.Id equals w.PageName
					join u in users on ls.User.Name equals u.UserName into uu // Some wiki users were never in the forums, and therefore could not be imported (no password for instance)
					from u in uu.DefaultIfEmpty()
					select new { Sub = ls, System = s, Wiki = w, Submitter = u })
					.ToList();

				foreach (var legacySubmission in lSubsWithSystem)
				{
					string pageName = LinkConstants.SubmissionWikiPage + legacySubmission.Sub.Id;

					GameSystemFrameRate systemFrameRate;

					if (legacySubmission.Sub.GameVersion.ToLower().Contains("euro"))
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
						GameName = ImportHelper.FixString(legacySubmission.Sub.GameName),
						GameVersion = legacySubmission.Sub.GameVersion,
						Frames = legacySubmission.Sub.Frames,
						Status = ConvertStatus(legacySubmission.Sub.Status),
						RomName = legacySubmission.Sub.RomName,
						RerecordCount = legacySubmission.Sub.Rerecord,
						MovieFile = legacySubmission.Sub.Content,
						IntendedTierId = legacySubmission.Sub.IntendedTier,
						GameId = legacySubmission.Sub.GameNameId ?? -1, // Placeholder game if not present
						RomId = -1 // Legacy system had no notion of Rom for submissions
					};

					// For now at least
					if (legacySubmission.Submitter != null)
					{
						var subAuthor = new SubmissionAuthor
						{
							SubmissionId = submission.Id,
							Submisison = submission,
							UserId = legacySubmission.Submitter.Id,
							Author = legacySubmission.Submitter
						};

						submission.SubmissionAuthors.Add(subAuthor);
						submissionAuthors.Add(subAuthor);
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
				nameof(Submission.RomId)
			};

			submissions.BulkInsert(connectionStr, subColumns, nameof(ApplicationDbContext.Submissions), bulkCopyTimeout: 600);

			var subAuthorColumns = new[]
			{
				nameof(SubmissionAuthor.UserId),
				nameof(SubmissionAuthor.SubmissionId)
			};

			submissionAuthors.BulkInsert(connectionStr, subAuthorColumns, nameof(ApplicationDbContext.SubmissionAuthors));
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
	}
}
