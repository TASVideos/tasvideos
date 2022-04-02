using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

[RequirePermission(PermissionTo.UploadUserFiles)]
public class UploadModel : BasePageModel
{
	private readonly IUserFiles _userFiles;
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;

	public UploadModel(
		IUserFiles userFiles,
		ApplicationDbContext db,
		ExternalMediaPublisher publisher)
	{
		_userFiles = userFiles;
		_db = db;
		_publisher = publisher;
	}

	[BindProperty]
	public UserFileUploadModel UserFile { get; set; } = new();

	public int StorageUsed { get; set; }

	public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();

	public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();

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
			ModelState.AddModelError(
				$"{nameof(UserFile)}.{nameof(UserFile.File)}",
				"Compressed files are not supported.");
			return Page();
		}

		var fileExt = UserFile.File.FileExtension();

		if (!await _userFiles.IsSupportedFileExtension(fileExt))
		{
			ModelState.AddModelError(
				$"{nameof(UserFile)}.{nameof(UserFile.File)}",
				$"Unsupported file type: {fileExt}");
			return Page();
		}

		if (!await _userFiles.SpaceAvailable(User.GetUserId(), UserFile.File!.Length))
		{
			ModelState.AddModelError(
				$"{nameof(UserFile)}.{nameof(UserFile.File)}",
				"File exceeds your available storage space. Remove unecessary files and try again.");
			return Page();
		}

		byte[] actualFileData = await UserFile.File.ActualFileData();
		var (id, parseResult) = await _userFiles.Upload(User.GetUserId(), new(
			UserFile.Title,
			UserFile.Description,
			UserFile.SystemId,
			UserFile.GameId,
			actualFileData,
			UserFile.File.FileName,
			UserFile.Hidden));

		if (parseResult is not null && !parseResult.Success)
		{
			ModelState.AddParseErrors(parseResult, $"{nameof(UserFile)}.{nameof(UserFile.File)}");
			await Initialize();
			return Page();
		}

		await _publisher.SendUserFile(
			UserFile.Hidden,
			$"New user file uploaded by {User.Name()}",
			$"/UserFiles/Info/{id}",
			$"{UserFile.Title}");

		return BasePageRedirect("/Profile/UserFiles");
	}

	private async Task Initialize()
	{
		StorageUsed = await _userFiles.StorageUsed(User.GetUserId());

		AvailableSystems = UiDefaults.DefaultEntry.Concat(await _db.GameSystems
			.OrderBy(s => s.Code)
			.Select(s => new SelectListItem
			{
				Value = s.Id.ToString(),
				Text = s.Code
			})
			.ToListAsync());

		AvailableGames = UiDefaults.DefaultEntry.Concat(await _db.Games
			.OrderBy(g => g.SystemId)
			.ThenBy(g => g.DisplayName)
			.ToDropDown()
			.ToListAsync());
	}
}
