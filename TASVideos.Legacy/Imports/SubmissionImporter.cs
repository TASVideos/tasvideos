using System;
using System.Linq;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Legacy.Data.Site;

using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Data;

namespace TASVideos.Legacy.Imports
{
	public static class SubmissionImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			// TODO:
			// id mismatch! old data has id gps
			// authors that are not submitters
			// submitters not in forum 
			// judge
			// publisher
			// Game data
			// MovieExtension

			var legacySubmissions = legacySiteContext.Submissions
				.Where(s => s.Id > 0)
				.ToList();

			var legacySiteUsers = legacySiteContext.Users.ToList();
			var users = context.Users.ToList();

			var submissionWikis = context.WikiPages
				.ThatAreCurrentRevisions()
				.Where(w => w.PageName.StartsWith(LinkConstants.SubmissionWikiPage))
				.ToList();
			var systems = context.GameSystems.ToList();
			var systemFrameRates = context.GameSystemFrameRates.ToList();
			var tiers = context.Tiers.ToList();


			foreach (var legacySubmission in legacySubmissions)
			{
				InsertDummySubmission(legacySubmission.Id, context.Database.GetDbConnection().ConnectionString);
			}

			var newSubmissions = context.Submissions.ToList();

			foreach (var legacySubmission in legacySubmissions)
			{
				InsertDummySubmission(legacySubmission.Id, context.Database.GetDbConnection().ConnectionString);
				var submission = newSubmissions.Single(s => s.Id == legacySubmission.Id);

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

				submission.WikiContent = submissionWikis.Single(w => w.PageName == pageName);
				submission.Submitter = submitter;
				submission.SystemId = system.Id;
				submission.System = system;
				submission.SystemFrameRateId = systemFrameRate.Id;
				submission.SystemFrameRate = systemFrameRate;
				submission.CreateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacySubmission.SubmissionDate);
				submission.CreateUserName = submitter?.UserName;
				submission.GameName = legacySubmission.GameName;
				submission.GameVersion = legacySubmission.GameVersion;
				submission.Frames = legacySubmission.Frames;
				submission.Status = ConvertStatus(legacySubmission.Status);
				submission.RomName = legacySubmission.RomName;
				submission.RerecordCount = legacySubmission.Rerecord;
				submission.MovieFile = legacySubmission.Content;
				submission.IntendedTier = legacySubmission.IntendedTier.HasValue
					? tiers.Single(t => t.Id == legacySubmission.IntendedTier)
					: null;
				// TODO:
				// Judge (if StatusBy and Status or judged_by
				// Publisher (if StatusBy and Status

				// For now at least
				if (submitter != null)
				{
					var subAuthor = new SubmissionAuthor
					{
						Submisison = submission,
						Author = submitter
					};

					submission.SubmissionAuthors.Add(subAuthor);
					context.SubmissionAuthors.Add(subAuthor);
				}
			}

			context.SaveChanges();

			var subs = context.Submissions.ToList();
			foreach (var sub in subs)
			{
				sub.GenerateTitle();
			}

			context.SaveChanges();
		}

		private static int InsertDummySubmission(int id, string connectionString)
		{
			int identity = 0;
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				using (var cmd = new SqlCommand
				{
					CommandText = $@"
SET IDENTITY_INSERT Submissions ON
INSERT INTO Submissions
(id, CreateTimeStamp, Frames, LastUpdateTimeStamp, RerecordCount, Status)
values
({id}, getdate(), 1, getdate(), 1, 1)",
					CommandType = CommandType.Text,
					Connection = sqlConnection
				})
				{
					sqlConnection.Open();
					identity = cmd.ExecuteNonQuery();
				}
			}

			return identity;
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
