using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

using TASVideos.Models;

namespace TASVideos.Pages.Submissions.Models
{
	public class SubmissionCreateModel
	{
		[Required]
		[Display(Name = "Game Version", Description = "Example: USA")]
		[StringLength(20)]
		public string GameVersion { get; set; }

		[Required]
		[Display(Name = "Game Name", Description = "Example: Mega Man 2")]
		[StringLength(100)]
		public string GameName { get; set; }

		[Display(Name = "Branch Name", Description = "Example: 100% or princess only; any% can usually be omitted)")]
		[StringLength(50)]
		public string Branch { get; set; }

		[Required]
		[Display(Name = "ROM filename", Description = "Example: Mega Man II (U) [!].nes")]
		[StringLength(100)]
		public string RomName { get; set; }

		[Display(Name = "Emulator and version", Description = "Example: BizHawk 2.2.1")]
		[StringLength(50)]
		public string Emulator { get; set; }

		[Url]
		[Display(Name = "Encode Embedded Link", Description = "Embedded link to a video of your movie, Ex: www.youtube.com/embed/0mregEW6kVU")]
		public string EncodeEmbedLink { get; set; }

		[Display(Name = "Author(s)")]
		[AtLeastOne(ErrorMessage = "A submission must have at least one author")]
		public IList<string> Authors { get; set; } = new List<string>();

		[Required]
		[Display(Name = "Comments and explanations")]
		public string Markup { get; set; }

		[Required]
		[Display(Name = "Movie file", Description = "Your movie packed in a ZIP file (max size: 150k)")]
		public IFormFile MovieFile { get; set; }
	}
}
