using System;
using TASVideos.Data.Entity;

namespace TASVideos.RazorPages.Pages.Submissions.Models
{
	public interface ISubmissionDisplay
	{
		SubmissionStatus Status { get; }
		DateTime Submitted { get; }
	}

	public static class SubmissionDisplayExtensions
	{
		public static int HoursRemainingForJudging(this ISubmissionDisplay submission)
		{
			if (submission.Status.CanBeJudged())
			{
				var diff = (DateTime.UtcNow - submission.Submitted).TotalHours;
				return SiteGlobalConstants.MinimumHoursBeforeJudgment - (int)diff;
			}

			return 0;
		}
	}
}
