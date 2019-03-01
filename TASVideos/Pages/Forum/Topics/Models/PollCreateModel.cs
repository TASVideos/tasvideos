using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Topics.Models
{
	public class PollCreateModel
	{
		[Display(Name = "Poll Question")]
		[StringLength(200, MinimumLength = 8)]
		public string Question { get; set; }

		[Display(Name = "Run poll for")]
		[MaxLength(365)]
		public int? DaysOpen { get; set; }

		public IEnumerable<string> PollOptions { get; set; } = new List<string>();
	}
}
