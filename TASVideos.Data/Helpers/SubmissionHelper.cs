using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc.Rendering;

using TASVideos.Data.Constants;
using TASVideos.Data.Entity;

namespace TASVideos.Data.Helpers
{
	public static class SubmissionHelper
	{
		/// <summary>
		/// Returns a list of all available statuses a submission could be set to
		/// Based on the user's permissions, submission status and date, and authors
		/// </summary>
		public static IEnumerable<SubmissionStatus> AvailableStatuses(
			SubmissionStatus currentStatus,
			IEnumerable<PermissionTo> userPermissions,
			DateTime submitDate,
			bool isAuthorOrSubmitter,
			bool isJudge)
		{
			// Published submissions can not be changed
			if (currentStatus == SubmissionStatus.Published)
			{
				return new List<SubmissionStatus>(); 
			}

			var perms = userPermissions.ToList();
			if (perms.Contains(PermissionTo.OverrideSubmissionStatus)
				&& currentStatus != SubmissionStatus.Published)
			{
				return Enum.GetValues(typeof(SubmissionStatus))
					.Cast<SubmissionStatus>()
					.Except(new[] { SubmissionStatus.Published }); // Published status must only be set when being published
			}

			var list = new List<SubmissionStatus>();

			if ((currentStatus == SubmissionStatus.JudgingUnderWay && isJudge) // The judge can set back to new if they claimed the submission and are now opting out
				|| (currentStatus == SubmissionStatus.Rejected && perms.Contains(PermissionTo.JudgeSubmissions)) // A judge can revive a rejected submission by setting it to new
				|| (currentStatus == SubmissionStatus.Accepted && isJudge))  // A judge can undo their judgment
			{
				list.Add(SubmissionStatus.New);
			}

			if (currentStatus == SubmissionStatus.New
				&& perms.Contains(PermissionTo.JudgeSubmissions)
				&& !isAuthorOrSubmitter) // A judge can claim a new run, unless they are an author or the submitter
			{
				list.Add(SubmissionStatus.JudgingUnderWay);
			}

			if ((currentStatus == SubmissionStatus.New || currentStatus == SubmissionStatus.JudgingUnderWay) // An author or a judge can set a submission to delayed so long as it does not have a judgment
				&& (perms.Contains(PermissionTo.JudgeSubmissions) || isAuthorOrSubmitter))
			{
				list.Add(SubmissionStatus.Delayed);
			}

			if ((currentStatus == SubmissionStatus.New || currentStatus == SubmissionStatus.JudgingUnderWay) // A judge can set a submission to needs more info so long as it does not have a judgment
				&& perms.Contains(PermissionTo.JudgeSubmissions))
			{
				list.Add(SubmissionStatus.NeedsMoreInfo);
			}

			if ((currentStatus == SubmissionStatus.New
				|| currentStatus == SubmissionStatus.JudgingUnderWay
				|| currentStatus == SubmissionStatus.Delayed
				|| currentStatus == SubmissionStatus.NeedsMoreInfo)
				&& isJudge && !isAuthorOrSubmitter
				&& submitDate > DateTime.UtcNow.AddHours(-SiteGlobalConstants.MinimumHoursBeforeJudgment)) // A judge can claim a new run, unless they are an author or the submitter
			{
				list.Add(SubmissionStatus.Accepted);
				list.Add(SubmissionStatus.Rejected);
			}

			if (currentStatus == SubmissionStatus.Accepted && perms.Contains(PermissionTo.PublishMovies)) // A publisher can set it to publication underway if it has been accepted
			{
				list.Add(SubmissionStatus.PublicationUnderway);
			}

			if ((perms.Contains(PermissionTo.JudgeSubmissions) || isAuthorOrSubmitter) // An author or a judge can cancel a submission if it is not had a verdict
				&& (currentStatus == SubmissionStatus.New
					|| currentStatus == SubmissionStatus.JudgingUnderWay
					|| currentStatus == SubmissionStatus.Delayed
					|| currentStatus == SubmissionStatus.NeedsMoreInfo))
			{
				list.Add(SubmissionStatus.Cancelled);
			}

			if (!list.Contains(currentStatus))
			{
				list.Add(currentStatus);
			}

			return list;
		}

		/// <summary>
		/// Determines if the link is in the form of valid submission link ex: 100S
		/// </summary>
		/// <returns>The id of the submission if it is a valid link, else null</returns>
		public static int? IsSubmissionLink(string link)
		{
			if (link?.EndsWith("S") ?? false)
			{
				var result = int.TryParse(link.Substring(0, link.Length - 1), out int id);
				if (result)
				{
					return id;
				}
			}

			return null;
		}

		/// <summary>
		/// Determines if the link is in the form of valid movie link ex: 100M
		/// </summary>
		/// <returns>The id of the movie if it is a valid link, else null</returns>
		public static int? IsPublicationLink(string link)
		{
			if (link?.EndsWith("M") ?? false)
			{
				var result = int.TryParse(link.Substring(0, link.Length - 1), out int id);
				if (result)
				{
					return id;
				}
			}

			return null;
		}

		/// <summary>
		/// Determines if the link is in the form of valid game page link ex: 100G
		/// </summary>
		/// <returns>The id of the movie if it is a valid link, else null</returns>
		public static int? IsGamePageLink(string link)
		{
			if (link?.EndsWith("G") ?? false)
			{
				var result = int.TryParse(link.Substring(0, link.Length - 1), out int id);
				if (result)
				{
					return id;
				}
			}

			return null;
		}

		public static readonly SelectListItem[] GameVersionOptions = 
		{
			new SelectListItem { Text = "unknown", Value = "unknown" },
			new SelectListItem { Text = "unknown v1.0", Value = "unknown v1.0" },
			new SelectListItem { Text = "unknown v1.1", Value = "unknown v1.1" },
			new SelectListItem { Text = "unknown,r0", Value = "unknown,r0" },
			new SelectListItem { Text = "unknown,r1", Value = "unknown,r1" },
			new SelectListItem { Text = "unknown,r2", Value = "unknown,r2" },
			new SelectListItem { Text = "unknown PRG0", Value = "unknown PRG0" },
			new SelectListItem { Text = "unknown PRG1", Value = "unknown PRG1" },
			new SelectListItem { Text = "unknown PRG2", Value = "unknown PRG2" },
			new SelectListItem { Text = "any", Value = "any" },
			new SelectListItem { Text = "any v1.0", Value = "any v1.0" },
			new SelectListItem { Text = "any v1.1", Value = "any v1.1" },
			new SelectListItem { Text = "any,r0", Value = "any,r0" },
			new SelectListItem { Text = "any,r1", Value = "any,r1" },
			new SelectListItem { Text = "any,r2", Value = "any,r2" },
			new SelectListItem { Text = "any PRG0", Value = "any PRG0" },
			new SelectListItem { Text = "any PRG1", Value = "any PRG1" },
			new SelectListItem { Text = "any PRG2", Value = "any PRG2" },
			new SelectListItem { Text = "Europe", Value = "Europe" },
			new SelectListItem { Text = "Europe v1.0", Value = "Europe v1.0" },
			new SelectListItem { Text = "Europe v1.1", Value = "Europe v1.1" },
			new SelectListItem { Text = "Europe,r0", Value = "Europe,r0" },
			new SelectListItem { Text = "Europe,r1", Value = "Europe,r1" },
			new SelectListItem { Text = "Europe,r2", Value = "Europe,r2" },
			new SelectListItem { Text = "Europe PRG0", Value = "Europe PRG0" },
			new SelectListItem { Text = "Europe PRG1", Value = "Europe PRG1" },
			new SelectListItem { Text = "Europe PRG2", Value = "Europe PRG2" },
			new SelectListItem { Text = "FDS", Value = "FDS" },
			new SelectListItem { Text = "FDS v1.0", Value = "FDS v1.0" },
			new SelectListItem { Text = "FDS v1.1", Value = "FDS v1.1" },
			new SelectListItem { Text = "FDS,r0", Value = "FDS,r0" },
			new SelectListItem { Text = "FDS,r1", Value = "FDS,r1" },
			new SelectListItem { Text = "FDS,r2", Value = "FDS,r2" },
			new SelectListItem { Text = "FDS PRG0", Value = "FDS PRG0" },
			new SelectListItem { Text = "FDS PRG1", Value = "FDS PRG1" },
			new SelectListItem { Text = "FDS PRG2", Value = "FDS PRG2" },
			new SelectListItem { Text = "JPN", Value = "JPN" },
			new SelectListItem { Text = "JPN v1.0", Value = "JPN v1.0" },
			new SelectListItem { Text = "JPN v1.1", Value = "JPN v1.1" },
			new SelectListItem { Text = "JPN,r0", Value = "JPN,r0" },
			new SelectListItem { Text = "JPN,r1", Value = "JPN,r1" },
			new SelectListItem { Text = "JPN,r2", Value = "JPN,r2" },
			new SelectListItem { Text = "JPN PRG0", Value = "JPN PRG0" },
			new SelectListItem { Text = "JPN PRG1", Value = "JPN PRG1" },
			new SelectListItem { Text = "JPN PRG2", Value = "JPN PRG2" },
			new SelectListItem { Text = "JPN/USA", Value = "JPN/USA" },
			new SelectListItem { Text = "JPN/USA v1.0", Value = "JPN/USA v1.0" },
			new SelectListItem { Text = "JPN/USA v1.1", Value = "JPN/USA v1.1" },
			new SelectListItem { Text = "JPN/USA,r0", Value = "JPN/USA,r0" },
			new SelectListItem { Text = "JPN/USA,r1", Value = "JPN/USA,r1" },
			new SelectListItem { Text = "JPN/USA,r2", Value = "JPN/USA,r2" },
			new SelectListItem { Text = "JPN/USA PRG0", Value = "JPN/USA PRG0" },
			new SelectListItem { Text = "JPN/USA PRG1", Value = "JPN/USA PRG1" },
			new SelectListItem { Text = "JPN/USA PRG2", Value = "JPN/USA PRG2" },
			new SelectListItem { Text = "USA", Value = "USA" },
			new SelectListItem { Text = "USA v1.0", Value = "USA v1.0" },
			new SelectListItem { Text = "USA v1.1", Value = "USA v1.1" },
			new SelectListItem { Text = "USA,r0", Value = "USA,r0" },
			new SelectListItem { Text = "USA,r1", Value = "USA,r1" },
			new SelectListItem { Text = "USA,r2", Value = "USA,r2" },
			new SelectListItem { Text = "USA PRG0", Value = "USA PRG0" },
			new SelectListItem { Text = "USA PRG1", Value = "USA PRG1" },
			new SelectListItem { Text = "USA PRG2", Value = "USA PRG2" },
			new SelectListItem { Text = "USA/Europe", Value = "USA/Europe" },
			new SelectListItem { Text = "USA/Europe v1.0", Value = "USA/Europe v1.0" },
			new SelectListItem { Text = "USA/Europe v1.1", Value = "USA/Europe v1.1" },
			new SelectListItem { Text = "USA/Europe,r0", Value = "USA/Europe,r0" },
			new SelectListItem { Text = "USA/Europe,r1", Value = "USA/Europe,r1" },
			new SelectListItem { Text = "USA/Europe,r2", Value = "USA/Europe,r2" },
			new SelectListItem { Text = "USA/Europe PRG0", Value = "USA/Europe PRG0" },
			new SelectListItem { Text = "USA/Europe PRG1", Value = "USA/Europe PRG1" },
			new SelectListItem { Text = "USA/Europe PRG2", Value = "USA/Europe PRG2" },
		};
	}
}
