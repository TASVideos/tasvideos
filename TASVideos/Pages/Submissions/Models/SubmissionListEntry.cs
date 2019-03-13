using System;
using System.ComponentModel.DataAnnotations;

using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionListEntry
	{
		[Display(Name = "Movie name")]
		public string Title => $"{System} {GameName}"
		+ (!string.IsNullOrWhiteSpace(Branch) ? $" \"{Branch}\" " : "")
		+ $" in {Time:g}";

		[Display(Name = "Author")]
		public string Author { get; set; }

		[Display(Name = "Submitted")]
		public DateTime Submitted { get; set; }

		[Display(Name = "Status")]
		public SubmissionStatus Status { get; set; }

		public int Id { get; set; }
		public string System { get; set; }
		public string GameName { get; set; }
		public TimeSpan Time { get; set; }
		public string Branch { get; set; }
	}
}
