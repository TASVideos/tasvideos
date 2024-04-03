using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity.Awards;

namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class CreateCategoryModel(IAwards awards, IMediaFileUploader mediaFileUploader) : BasePageModel
{
	public List<SelectListItem> AvailableAwardTypes { get; set; } = Enum
		.GetValues<AwardType>()
		.ToDropDown();

	[BindProperty]
	public CreateAwardCategoryModel AwardCategory { get; set; } = new();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (!AwardCategory.BaseImage!.FileName.EndsWith(".png"))
		{
			ModelState.AddModelError($"{nameof(AwardCategory)}.{nameof(AwardCategory.BaseImage)}", "Invalid image type, must be .png");
			return Page();
		}

		await mediaFileUploader.UploadAwardImage(
			AwardCategory.BaseImage!,
			AwardCategory.BaseImage2X!,
			AwardCategory.BaseImage4X!,
			AwardCategory.ShortName);

		var result = await awards.AddAwardCategory(
			AwardCategory.Type,
			AwardCategory.ShortName,
			AwardCategory.Description);

		if (!result)
		{
			ModelState.AddModelError("", "An award with this short name and description already exist");
		}

		return BasePageRedirect("Index");
	}

	public class CreateAwardCategoryModel
	{
		public AwardType Type { get; init; }

		[StringLength(25)]
		public string ShortName { get; init; } = "";

		[StringLength(50)]
		public string Description { get; init; } = "";

		[Required]
		public IFormFile? BaseImage { get; init; }

		[Required]
		public IFormFile? BaseImage2X { get; init; }

		[Required]
		public IFormFile? BaseImage4X { get; init; }
	}
}
