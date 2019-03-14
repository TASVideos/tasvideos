using System;
using System.ComponentModel.DataAnnotations;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionListEntry : ITimeable
	{
		[Display(Name = "Movie name")]
		public string Title => $"{System} {GameName}"
		+ (!string.IsNullOrWhiteSpace(Branch) ? $" \"{Branch}\" " : "")
		+ $" in {this.Time():g}";

		[Display(Name = "Author")]
		public string Author { get; set; }

		[Sortable]
		[Display(Name = "Submitted")]
		public DateTime Submitted { get; set; }

		[Sortable]
		[Display(Name = "Status")]
		public SubmissionStatus Status { get; set; }

		public int Id { get; set; }
		public string System { get; set; }
		public string GameName { get; set; }
		public string Branch { get; set; }

		public int Frames { get; set; }
		public double FrameRate { get; set; }
	}
}
