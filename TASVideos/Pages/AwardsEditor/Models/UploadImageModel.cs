using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.AwardsEditor.Models;

public class UploadImageViewModel
{
	[Required]
	public string? Award { get; init; }

	[Required]
	public IFormFile? BaseImage { get; init; }

	[Required]
	public IFormFile? BaseImage2X { get; init; }

	[Required]
	public IFormFile? BaseImage4X { get; init; }
}
