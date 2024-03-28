using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.ExternalMediaPublisher;
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

	public List<SelectListItem> AvailableSystems { get; set; } = [];

	public List<SelectListItem> AvailableGames { get; set; } = [];

	public List<string> SupportedFileExtensions { get; set; } = [];

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

		AvailableSystems = (await db.GameSystems
			.OrderBy(s => s.Code)
			.ToDropDownWithId()
			.ToListAsync())
			.WithDefaultEntry();

		AvailableGames = (await db.Games
			.OrderBy(g => g.DisplayName)
			.ToDropDown()
			.ToListAsync())
			.WithDefaultEntry();
	}
}
