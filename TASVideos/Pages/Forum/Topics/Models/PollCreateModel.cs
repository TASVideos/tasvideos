using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Pages.Forum.Topics.Models
{
	public class PollCreateModel
	{
		[Display(Name = "Question")]
		[StringLength(200, MinimumLength = 8)]
		public string Question { get; set; }

		[Display(Name = "Days to Run for", Description = "0 or empty for a never-ending poll")]
		public int? DaysOpen { get; set; }

		[Display(Name = "Options")]
		public IEnumerable<string> PollOptions { get; set; } = new List<string> { "", "" };

		public bool IsValid => 
			!string.IsNullOrWhiteSpace(Question)
			&& Question.Length <= 200
			&& PollOptions != null
			&& PollOptions.Count(o => !string.IsNullOrWhiteSpace(o)) > 2
			&& PollOptions.All(o => o != null && o.Length <= 250);
	}
}
