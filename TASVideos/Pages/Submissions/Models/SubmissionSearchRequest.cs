using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionSearchRequest
	{
		public int? Limit { get; set; }
		public DateTime? Cutoff { get; set; } // Only submissions submitted after this date
		public string User { get; set; }
		public IEnumerable<SubmissionStatus> StatusFilter { get; set; } = new List<SubmissionStatus>();

		public static IEnumerable<SubmissionStatus> Default => new List<SubmissionStatus>()
		{
			SubmissionStatus.New,
			SubmissionStatus.JudgingUnderWay,
			SubmissionStatus.Accepted,
			SubmissionStatus.PublicationUnderway,
			SubmissionStatus.NeedsMoreInfo,
			SubmissionStatus.Delayed
		};

		public static IEnumerable<SubmissionStatus> All => Enum
			.GetValues(typeof(SubmissionStatus))
			.Cast<SubmissionStatus>()
			.ToList();
	}
}
