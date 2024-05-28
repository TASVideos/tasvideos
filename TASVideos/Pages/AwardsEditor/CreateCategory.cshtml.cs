using TASVideos.Data.Entity.Awards;

namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class CreateCategoryModel(IAwards awards, IMediaFileUploader mediaFileUploader) : BasePageModel
{
	public List<SelectListItem> AvailableAwardTypes => Enum
		.GetValues<AwardType>()
		.ToDropDown();

	[BindProperty]
	public AwardType Type { get; init; }

	[BindProperty]
	[StringLength(25)]
	public string ShortName { get; init; } = "";

	[BindProperty]
	[StringLength(50)]
	public string Description { get; init; } = "";

	[BindProperty]
	[Required]
	public IFormFile? BaseImage { get; init; }

	[BindProperty]
	[Required]
	public IFormFile? BaseImage2X { get; init; }

	[BindProperty]
	[Required]
	public IFormFile? BaseImage4X { get; init; }

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (!BaseImage!.FileName.EndsWith(".png"))
		{
			ModelState.AddModelError($"{nameof(BaseImage)}", "Invalid image type, must be .png");
			return Page();
		}

		await mediaFileUploader.UploadAwardImage(
			BaseImage!, BaseImage2X!, BaseImage4X!, ShortName);

		var result = await awards.AddAwardCategory(Type, ShortName, Description);
		if (!result)
		{
			ModelState.AddModelError("", "An award with this short name and description already exist");
		}

		return BasePageRedirect("Index");
	}
}
