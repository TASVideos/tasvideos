using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.SetPublicationClass)]
public class EditClassModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;

	public EditClassModel(
		ApplicationDbContext db,
		ExternalMediaPublisher publisher,
		IPublicationMaintenanceLogger publicationMaintenanceLogger)
	{
		_db = db;
		_publisher = publisher;
		_publicationMaintenanceLogger = publicationMaintenanceLogger;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public PublicationClassEditModel Publication { get; set; } = new();

	[BindProperty]
	public string Title { get; set; } = "";

	public IEnumerable<SelectListItem> AvailableClasses { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		var publication = await _db.Publications
			.Where(p => p.Id == Id)
			.Select(p => new PublicationClassEditModel
			{
				Title = p.Title,
				ClassId = p.PublicationClassId
			})
			.SingleOrDefaultAsync();

		if (publication is null)
		{
			return NotFound();
		}

		Publication = publication;
		Title = Publication.Title;
		await PopulateAvailableClasses();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			await PopulateAvailableClasses();
			return Page();
		}

		var publication = await _db.Publications
			.Include(p => p.PublicationClass)
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (publication is null)
		{
			return NotFound();
		}

		var publicationClass = await _db.PublicationClasses
			.SingleOrDefaultAsync(t => t.Id == Publication.ClassId);

		if (publicationClass is null)
		{
			return NotFound();
		}

		if (publication.PublicationClassId != Publication.ClassId)
		{
			var originalClass = publication.PublicationClass!.Name;
			publication.PublicationClassId = Publication.ClassId;

			var log = $"{Id}M Class changed from {originalClass} to {publicationClass.Name}";
			await _publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);

			var result = await ConcurrentSave(_db, log, "Unable to update Publication Class");
			if (result)
			{
				await _publisher.SendPublicationEdit(
					$"{log} by {User.Name()}",
					Title,
					$"{Id}M");
			}
		}

		return RedirectToPage("Edit", new { Id });
	}

	private async Task PopulateAvailableClasses()
	{
		AvailableClasses = await _db.PublicationClasses
			.ToDropdown()
			.ToListAsync();
	}
}
