namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class UploadImageModel(IMediaFileUploader mediaFileUploader, IAwards awards) : BasePageModel
{
	[FromRoute]
	public int Year { get; set; }

	[BindProperty]
	public string Award { get; init; } = "";

	[BindProperty]
	[Required]
	public IFormFile? BaseImage { get; init; }

	[BindProperty]
	[Required]
	public IFormFile? BaseImage2X { get; init; }

	[BindProperty]
	[Required]
	public IFormFile? BaseImage4X { get; init; }

	public List<SelectListItem> AvailableAwardCategories { get; set; } = [];

	public async Task OnGet() => await Initialize();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		var exists = await awards.CategoryExists(Award);
		if (!exists)
		{
			ModelState.AddModelError("", "Award does not exist.");
			await Initialize();
			return Page();
		}

		await mediaFileUploader.UploadAwardImage(
			BaseImage!, BaseImage2X!, BaseImage4X!, Award, Year);

		return BasePageRedirect("Index", new { Year });
	}

	private async Task Initialize()
	{
		AvailableAwardCategories = (await awards.AwardCategories()
			.ToDropdownList(Year))
			.WithDefaultEntry();
	}
}
