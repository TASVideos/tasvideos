using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity.Awards;

namespace TASVideos.Pages.AwardsEditor.Models;

public class CreateAwardCategoryModel
{
	public int Id { get; set; }
	public AwardType Type { get; set; }

	[Required]
	[StringLength(25)]
	public string ShortName { get; set; } = "";

	[Required]
	[StringLength(50)]
	public string Description { get; set; } = "";

	[Required]
	public IFormFile? BaseImage { get; set; }

	[Required]
	public IFormFile? BaseImage2X { get; set; }

	[Required]
	public IFormFile? BaseImage4X { get; set; }
}
