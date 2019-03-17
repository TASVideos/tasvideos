using System;
using System.ComponentModel.DataAnnotations;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionListEntry : ITimeable
	{
		[Sortable]
		public string System { get; set; }

		[Display(Name = "Movie name")]
		public string Title => $"{GameName}"
		+ (!string.IsNullOrWhiteSpace(Branch) ? $" \"{Branch}\" " : "");

		[Display(Name = "Author")]
		public string Author { get; set; }

		[Sortable]
		[Display(Name = "Submitted")]
		public DateTime Submitted { get; set; }

		[Sortable]
		[Display(Name = "Status")]
		public SubmissionStatus Status { get; set; }

		[Display(Name = "Time")]
		public TimeSpan Time => this.Time();

		public int Id { get; set; }
		public string GameName { get; set; }
		public string Branch { get; set; }

		public int Frames { get; set; }
		public double FrameRate { get; set; }
	}
}
