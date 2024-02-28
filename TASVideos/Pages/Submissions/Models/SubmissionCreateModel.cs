using System.ComponentModel.DataAnnotations;
using TASVideos.Models;
using TASVideos.Models.ValidationAttributes;

namespace TASVideos.Pages.Submissions.Models;

public class SubmissionCreateModel
{
	[Display(Name = "Game Version", Description = "Example: USA")]
	[StringLength(20)]
	public string GameVersion { get; set; } = "";

	[Display(Name = "Game Name", Description = "Example: Mega Man 2")]
	[StringLength(100)]
	public string GameName { get; set; } = "";

	[Display(Name = "Goal Name", Description = "Example: 100% or princess only; any% can usually be omitted")]
	[StringLength(50)]
	public string? Branch { get; set; }

	[Display(Name = "ROM filename", Description = "Example: Mega Man II (U) [!].nes")]
	[StringLength(100)]
	public string RomName { get; set; } = "";

	[Display(Name = "Emulator and version", Description = "Example: BizHawk 2.8.0")]
	[StringLength(50)]
	public string? Emulator { get; set; }

	[Url]
	[Display(Name = "Encode Embedded Link", Description = "Embedded link to a video of your movie, Ex: www.youtube.com/embed/0mregEW6kVU")]
	public string? EncodeEmbedLink { get; set; }

	[Display(Name = "Author(s)")]
	[MinLength(1)]
	public IList<string> Authors { get; set; } = new List<string>();

	[Display(Name = "External Authors", Description = "Only authors not registered for TASVideos should be listed here. If multiple authors, separate the names with a comma.")]
	public string? AdditionalAuthors { get; set; }

	[DoNotTrim]
	[Display(Name = "Comments and explanations")]
	public string Markup { get; set; } = "";

	[Required]
	[Display(Name = "Movie file", Description = "Your movie packed in a ZIP file (max size: 500k)")]
	public IFormFile? MovieFile { get; set; }

	[MustBeTrue(ErrorMessage = "You must read and follow the instructions.")]
	public bool AgreeToInstructions { get; set; }

	[MustBeTrue(ErrorMessage = "You must agree to the license.")]
	public bool AgreeToLicense { get; set; }
}
