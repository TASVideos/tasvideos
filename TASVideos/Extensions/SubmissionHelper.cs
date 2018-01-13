using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data.Entity;

namespace TASVideos.Extensions
{
    public static class SubmissionHelper
	{
		public const int MinimumHoursBeforeJudgement = 72;

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
				|| (currentStatus == SubmissionStatus.Accepted && isJudge))  // A judge can undo their judgement
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
				&& submitDate > DateTime.UtcNow.AddHours(-MinimumHoursBeforeJudgement)) // A judge can claim a new run, unless they are an author or the submitter
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
	}
}
