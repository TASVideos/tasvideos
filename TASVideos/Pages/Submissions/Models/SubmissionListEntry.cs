using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TASVideos.Common;
using TASVideos.Core;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionListEntry : ITimeable, ISubmissionDisplay
	{
		[Sortable]
		public string? System { get; set; }

		[Sortable]
		[Display(Name = "Game")]
		public string? GameName { get; set; }

		[Sortable]
		public string? Branch { get; set; }

		[Display(Name = "Time")]
		public TimeSpan Time => this.Time();

		[Display(Name = "By")]
		public IEnumerable<string>? Authors { get; set; }
		[TableIgnore]
		public string? AdditionalAuthors { get; set; }

		[Sortable]
		[Display(Name = "Date")]
		public DateTime Submitted { get; set; }

		[Sortable]
		[Display(Name = "Status")]
		public SubmissionStatus Status { get; set; }

		[TableIgnore]
		public int Id { get; set; }

		[TableIgnore]
		public int Frames { get; set; }

		[TableIgnore]
		public double FrameRate { get; set; }

		[TableIgnore]
		public string? Judge { get; set; }

		[TableIgnore]
		public string? Publisher { get; set; }

		[TableIgnore]
		public string? IntendedClass { get; set; }
	}

	public class SubmissionPageOf<T> : PageOf<T>
	{
		public SubmissionPageOf(IEnumerable<T> items)
			: base(items)
		{
		}

		public IEnumerable<int> Years { get; set; } = new List<int>();
		public IEnumerable<SubmissionStatus> StatusFilter { get; set; } = new List<SubmissionStatus>();
		public string? System { get; set; }
		public string? User { get; set; }

		public static new SubmissionPageOf<T> Empty() => new (Enumerable.Empty<T>());
	}
}
