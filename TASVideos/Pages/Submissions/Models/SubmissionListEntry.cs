using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionListEntry : ITimeable
	{
		[Sortable]
		public string System { get; set; }

		[Sortable]
		[Display(Name = "Title")]
		public string GameName { get; set; }

		[Sortable]
		public string Branch { get; set; }

		[Display(Name = "Time")]
		public TimeSpan Time => this.Time();

		[Display(Name = "Author")]
		public string Author { get; set; }

		[Sortable]
		[Display(Name = "Submitted")]
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
	}

	public class SubmissionPageOf<T> : PageOf<T>
	{
		public SubmissionPageOf(IEnumerable<T> items)
			: base(items)
		{
		}

		public IEnumerable<int> Years { get; set; } = new List<int>();
		public IEnumerable<SubmissionStatus> StatusFilter { get; set; } = new List<SubmissionStatus>();
		public string System { get; set; }
		public string User { get; set; }
	}
}
