using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionSearchRequest : ISubmissionFilter
	{
		public DateTime? StartDate { get; set; } // Only submissions submitted after this date
		public string User { get; set; }

		[Display(Name = "Status Filter")]
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
