using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionEditModel : SubmissionDisplayModel
	{
		public string Markup { get; set; }

		[StringLength(1000)]
		[Display(Name = "Revision Message")]
		public string RevisionMessage { get; set; }

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; set; }

		[Display(Name = "Replace Movie file", Description = "Your movie packed in a ZIP file (max size: 150k)")]
		public IFormFile MovieFile { get; set; }

		[Display(Name = "Intended Tier")]
		public int? TierId { get; set; }

		[Display(Name = "Reason")]
		public int? RejectionReason { get; set; }
	}
}
