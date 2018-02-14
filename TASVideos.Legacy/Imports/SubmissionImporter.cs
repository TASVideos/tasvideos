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
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class SubmissionImporter
	{
		public static void Import(
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
				.Where(s => s.Id > 0)
				.ToList();

			var legacySiteUsers = legacySiteContext.Users.ToList();
			var users = context.Users.ToList();

			var submissionWikis = context.WikiPages
				.ThatAreNotDeleted()
				.ThatAreCurrentRevisions()
				.Where(w => w.PageName.StartsWith(LinkConstants.SubmissionWikiPage))
				.Select(s => new { s.Id, s.PageName })
				.ToList();
			var systems = context.GameSystems.ToList();
			var systemFrameRates = context.GameSystemFrameRates.ToList();

			var submissions = new List<Submission>();
			var submissionAuthors = new List<SubmissionAuthor>();
			foreach (var legacySubmission in legacySubmissions)
			{
				string pageName = LinkConstants.SubmissionWikiPage + legacySubmission.Id;
				string submitterName = legacySiteUsers.Single(u => u.Id == legacySubmission.UserId).Name;
				User submitter = users.SingleOrDefault(u => u.UserName == submitterName); // Some wiki users were never in the forums, and therefore could not be imported (no password for instance)

				var system = systems.Single(s => s.Id == legacySubmission.SystemId);
				GameSystemFrameRate systemFrameRate;

				if (legacySubmission.GameVersion.ToLower().Contains("euro"))
				{
					systemFrameRate = systemFrameRates
						.SingleOrDefault(sf => sf.GameSystemId == system.Id && sf.RegionCode == "PAL")
						?? systemFrameRates.Single(sf => sf.GameSystemId == system.Id && sf.RegionCode == "NTSC");
				}
				else
				{
					systemFrameRate = systemFrameRates
						.Single(sf => sf.GameSystemId == system.Id && sf.RegionCode == "NTSC");
				}

				var submissionWiki = submissionWikis.Single(w => w.PageName == pageName);
				var submission = new Submission
				{
					Id = legacySubmission.Id,
					WikiContentId = submissionWiki.Id,
					SubmitterId = submitter?.Id,
					Submitter = submitter,
					SystemId = system.Id,
					System = system,
					SystemFrameRateId = systemFrameRate.Id,
					SystemFrameRate = systemFrameRate,
					CreateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacySubmission.SubmissionDate),
					CreateUserName = submitter?.UserName,
					LastUpdateTimeStamp = DateTime.UtcNow, // TODO
					GameName = legacySubmission.GameName,
					GameVersion = legacySubmission.GameVersion,
					Frames = legacySubmission.Frames,
					Status = ConvertStatus(legacySubmission.Status),
					RomName = legacySubmission.RomName,
					RerecordCount = legacySubmission.Rerecord,
					MovieFile = legacySubmission.Content,
					IntendedTierId = legacySubmission.IntendedTier,
					GameId = legacySubmission.GameNameId ?? -1, // Placeholder game if not present
					RomId = -1 // Legacy system had no notion of Rom for submissions
				};

				// For now at least
				if (submitter != null)
				{
					var subAuthor = new SubmissionAuthor
					{
						SubmissionId = submission.Id,
						Submisison = submission,
						UserId = submitter.Id,
						Author = submitter
					};

					submission.SubmissionAuthors.Add(subAuthor);
					submissionAuthors.Add(subAuthor);
				}

				submission.GenerateTitle();
				submissions.Add(submission);
			}

			var subCopyParams = new[]
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

			using (var subSqlCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString, SqlBulkCopyOptions.KeepIdentity))
			{
				subSqlCopy.DestinationTableName = "[Submissions]";
				subSqlCopy.BatchSize = 10000;

				foreach (var param in subCopyParams)
				{
					subSqlCopy.ColumnMappings.Add(param, param);
				}

				using (var reader = ObjectReader.Create(submissions, subCopyParams))
				{
					subSqlCopy.WriteToServer(reader);
				}
			}

			var subAuthorCopyParams = new[]
			{
				nameof(SubmissionAuthor.UserId),
				nameof(SubmissionAuthor.SubmissionId)
			};

			using (var subAuthorSqlCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString))
			{
				subAuthorSqlCopy.DestinationTableName = "[SubmissionAuthors]";
				subAuthorSqlCopy.BatchSize = 10000;

				foreach (var param in subAuthorCopyParams)
				{
					subAuthorSqlCopy.ColumnMappings.Add(param, param);
				}

				using (var reader = ObjectReader.Create(submissionAuthors, subAuthorCopyParams))
				{
					subAuthorSqlCopy.WriteToServer(reader);
				}
			}
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
