using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionCatalogModel
	{
		public string Title { get; set; }

		[Display(Name = "Rom")]
		public int? RomId { get; set; }

		[Display(Name = "Game")]
		public int? GameId { get; set; }

		[Display(Name = "System")]
		public int? SystemId { get; set; }

		[Display(Name = "System Framerate")]
		public int? SystemFrameRateId { get; set; }
	}
}
