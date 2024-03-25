using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class UploadImageModel(IMediaFileUploader mediaFileUploader, IAwards awards) : BasePageModel
{
	[FromRoute]
	public int Year { get; set; }

	public List<SelectListItem> AvailableAwardCategories { get; set; } = [];

	[BindProperty]
	public UploadImageViewModel ImageToUpload { get; set; } = new();

	public async Task OnGet() => await Initialize();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		var exists = await awards.CategoryExists(ImageToUpload.Award);
		if (!exists)
		{
			ModelState.AddModelError("", "Award does not exist.");
			await Initialize();
			return Page();
		}

		await mediaFileUploader.UploadAwardImage(
			ImageToUpload.BaseImage!,
			ImageToUpload.BaseImage2X!,
			ImageToUpload.BaseImage4X!,
			ImageToUpload.Award,
			Year);

		return BasePageRedirect("Index", new { Year });
	}

	private async Task Initialize()
	{
		AvailableAwardCategories =
		[
			.. UiDefaults.DefaultEntry,
			.. await awards.AwardCategories()
				.OrderBy(c => c.Description)
				.ToDropdown(Year)
				.ToListAsync(),
		];
	}

	public class UploadImageViewModel
	{
		public string Award { get; init; } = "";

		[Required]
		public IFormFile? BaseImage { get; init; }

		[Required]
		public IFormFile? BaseImage2X { get; init; }

		[Required]
		public IFormFile? BaseImage4X { get; init; }
	}
}
