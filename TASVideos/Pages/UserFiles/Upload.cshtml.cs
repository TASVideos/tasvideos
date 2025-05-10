namespace TASVideos.Pages.UserFiles;

[RequirePermission(PermissionTo.UploadUserFiles)]
public class UploadModel(
	IUserFiles userFiles,
	ApplicationDbContext db,
	IExternalMediaPublisher publisher)
	: BasePageModel
{
	[BindProperty]
	[Required]
	public IFormFile? UserFile { get; init; }

	[BindProperty]
	[StringLength(255)]
	public string Title { get; init; } = "";

	[BindProperty]
	[DoNotTrim]
	public string Description { get; init; } = "";

	[BindProperty]
	public int? System { get; init; }

	[BindProperty]
	public int? Game { get; init; }

	[BindProperty]
	public bool Hidden { get; init; }

	public int StorageUsed { get; set; }

	public List<SelectListItem> AvailableSystems { get; set; } = [];

	public List<SelectListItem> AvailableGames { get; set; } = [];

	public List<string> SupportedFileExtensions { get; set; } = [];

	public async Task OnGet() => await Initialize();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await Initialize();
			return Page();
		}

		if (UserFile.IsCompressed())
		{
			await Initialize();
			ModelState.AddModelError(
				nameof(UserFile),
				"Compressed files are not supported.");
			return Page();
		}

		var fileExt = UserFile.FileExtension();

		if (!(await userFiles.SupportedFileExtensions()).Contains(fileExt))
		{
			await Initialize();
			ModelState.AddModelError(
				nameof(UserFile),
				$"Unsupported file type: {fileExt}");
			return Page();
		}

		if (!await userFiles.SpaceAvailable(User.GetUserId(), UserFile!.Length))
		{
			await Initialize();
			ModelState.AddModelError(
				nameof(UserFile),
				"File exceeds your available storage space. Remove unnecessary files and try again.");
			return Page();
		}

		byte[] fileData = (await UserFile.DecompressOrTakeRaw()).ToArray();

		var (id, parseResult) = await userFiles.Upload(User.GetUserId(), new(
			Title,
			Description,
			System,
			Game,
			fileData,
			UserFile.FileName,
			Hidden));

		if (parseResult is not null && !parseResult.Success)
		{
			await Initialize();
			ModelState.AddParseErrors(parseResult, $"{nameof(UserFile)}");
			return Page();
		}

		await publisher.SendUserFile(
			Hidden,
			$"New [user file]({{0}}) uploaded by {User.Name()}",
			id!.Value,
			Title);

		return BasePageRedirect("/Profile/UserFiles");
	}

	private async Task Initialize()
	{
		SupportedFileExtensions = (await userFiles.SupportedFileExtensions())
			.Select(s => s.Replace(".", ""))
			.ToList();

		StorageUsed = await userFiles.StorageUsed(User.GetUserId());

		AvailableSystems = (await db.GameSystems
			.ToDropDownListWithId())
			.WithDefaultEntry();

		AvailableGames = (await db.Games
			.ToDropDownList())
			.WithDefaultEntry();
	}
}
