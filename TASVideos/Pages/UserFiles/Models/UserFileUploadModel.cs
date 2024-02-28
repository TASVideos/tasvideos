using System.ComponentModel.DataAnnotations;
using TASVideos.Models;

namespace TASVideos.Pages.UserFiles.Models;

public class UserFileUploadModel
{
	[Required]
	public IFormFile? File { get; set; }

	[StringLength(255)]
	public string Title { get; set; } = "";

	[DoNotTrim]
	public string Description { get; set; } = "";

	[Display(Name = "System")]
	public int? SystemId { get; set; }

	[Display(Name = "Game")]
	public int? GameId { get; set; }

	public bool Hidden { get; set; }
}
