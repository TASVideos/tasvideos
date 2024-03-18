using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

[RequirePermission(PermissionTo.UploadUserFiles)]
public class UploadModel(
	IUserFiles userFiles,
	ApplicationDbContext db,
	ExternalMediaPublisher publisher)
	: BasePageModel
{
	[BindProperty]
	public UserFileUploadModel UserFile { get; set; } = new();

	public int StorageUsed { get; set; }

	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();

	public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();

	public IReadOnlyCollection<string> SupportedFileExtensions { get; set; } = new List<string>();

	public async Task OnGet()
	{
		await Initialize();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		if (UserFile.File.IsCompressed())
		{
			await Initialize();
			ModelState.AddModelError(
				$"{nameof(UserFile)}.{nameof(UserFile.File)}",
				"Compressed files are not supported.");
			return Page();
		}

		var fileExt = UserFile.File.FileExtension();

		if (!(await userFiles.SupportedFileExtensions()).Contains(fileExt))
		{
			await Initialize();
			ModelState.AddModelError(
				$"{nameof(UserFile)}.{nameof(UserFile.File)}",
				$"Unsupported file type: {fileExt}");
			return Page();
		}

		if (!await userFiles.SpaceAvailable(User.GetUserId(), UserFile.File!.Length))
		{
			await Initialize();
			ModelState.AddModelError(
				$"{nameof(UserFile)}.{nameof(UserFile.File)}",
				"File exceeds your available storage space. Remove unecessary files and try again.");
			return Page();
		}

		byte[] actualFileData = await UserFile.File.ActualFileData();
		var (id, parseResult) = await userFiles.Upload(User.GetUserId(), new(
			UserFile.Title,
			UserFile.Description,
			UserFile.SystemId,
			UserFile.GameId,
			actualFileData,
			UserFile.File.FileName,
			UserFile.Hidden));

		if (parseResult is not null && !parseResult.Success)
		{
			await Initialize();
			ModelState.AddParseErrors(parseResult, $"{nameof(UserFile)}.{nameof(UserFile.File)}");
			return Page();
		}

		await publisher.SendUserFile(
			UserFile.Hidden,
			$"New user file uploaded by {User.Name()}",
			$"New [user file]({{0}}) uploaded by {User.Name()}",
			$"/UserFiles/Info/{id}",
			$"{UserFile.Title}");

		return BasePageRedirect("/Profile/UserFiles");
	}

	private async Task Initialize()
	{
		SupportedFileExtensions = (await userFiles.SupportedFileExtensions())
			.Select(s => s.Replace(".", ""))
			.ToList();

		StorageUsed = await userFiles.StorageUsed(User.GetUserId());

		AvailableSystems = UiDefaults.DefaultEntry.Concat(await db.GameSystems
			.OrderBy(s => s.Code)
			.Select(s => new SelectListItem
			{
				Value = s.Id.ToString(),
				Text = s.Code
			})
			.ToListAsync());

		AvailableGames = UiDefaults.DefaultEntry.Concat(await db.Games
			.OrderBy(g => g.DisplayName)
			.ToDropDown()
			.ToListAsync());
	}
}
