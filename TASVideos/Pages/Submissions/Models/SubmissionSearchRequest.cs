using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.AspNetCore.Mvc.Rendering;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionSearchRequest : PagingModel, ISubmissionFilter
	{
		public SubmissionSearchRequest()
		{
			Sort = $"{nameof(SubmissionListEntry.Submitted)}";
		}

		public IEnumerable<int> Years { get; set; } = new List<int>();

		public IEnumerable<SelectListItem> AvailableYears => Enumerable
			.Range(2000, DateTime.UtcNow.Year + 1 - 2000)
			.OrderByDescending(n => n)
			.Select(n => new SelectListItem
			{
				Text = n.ToString(),
				Value = n.ToString()
			});

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
