using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.EditPublicationFiles)]
public class EditFilesModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IMediaFileUploader uploader,
	IPublicationMaintenanceLogger publicationMaintenanceLogger)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public string Title { get; set; } = "";

	public IReadOnlyCollection<PublicationFile> Files { get; set; } = new List<PublicationFile>();

	[Required]
	[BindProperty]
	[Display(Name = "New Screenshot")]
	public IFormFile? NewFile { get; set; }

	[BindProperty]
	[StringLength(250)]
	public string? Description { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var title = await db.Publications
			.Where(p => p.Id == Id)
			.Select(p => p.Title)
			.SingleOrDefaultAsync();

		if (title is null)
		{
			return NotFound();
		}

		Title = title;
		Files = await db.PublicationFiles
			.ForPublication(Id)
			.ToListAsync();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			Files = await db.PublicationFiles
				.ForPublication(Id)
				.ToListAsync();

			return Page();
		}

		var path = await uploader.UploadScreenshot(Id, NewFile!, Description);

		string log = $"Added Screenshot file {path}";
		SuccessStatusMessage(log);
		await publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
		await publisher.SendPublicationEdit(
			$"{Id}M edited by {User.Name()}",
			$"[{Id}M]({{0}}) edited by {User.Name()}",
			$"{log} | {Title}",
			$"{Id}M");

		return RedirectToPage("EditFiles", new { Id });
	}

	public async Task<IActionResult> OnPostDelete(int publicationFileId)
	{
		var file = await uploader.DeleteFile(publicationFileId);

		if (file is not null)
		{
			string log = $"Deleted {file.Type} file {file.Path}";
			SuccessStatusMessage(log);
			await publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
			await publisher.SendPublicationEdit(
				$"{Id}M edited by {User.Name()}",
				$"[{Id}M]({{0}}) edited by {User.Name()}",
				$"{log}",
				$"{Id}M");
		}

		return RedirectToPage("EditFiles", new { Id });
	}
}
