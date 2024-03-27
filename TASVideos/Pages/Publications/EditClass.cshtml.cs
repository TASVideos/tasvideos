using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.SetPublicationClass)]
public class EditClassModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IPublicationMaintenanceLogger publicationMaintenanceLogger)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public PublicationClassEditModel Publication { get; set; } = new();

	[BindProperty]
	public string Title { get; set; } = "";

	public List<SelectListItem> AvailableClasses { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var publication = await db.Publications
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

		var publication = await db.Publications
			.Include(p => p.PublicationClass)
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (publication is null)
		{
			return NotFound();
		}

		var publicationClass = await db.PublicationClasses
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
			await publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);

			var result = await ConcurrentSave(db, log, "Unable to update Publication Class");
			if (result)
			{
				await publisher.SendPublicationEdit(
					$"{log} by {User.Name()}",
					$"[{Id}M]({{0}}) Class changed from {originalClass} to {publicationClass.Name} by {User.Name()}",
					Title,
					$"{Id}M");
			}
		}

		return RedirectToPage("Edit", new { Id });
	}

	private async Task PopulateAvailableClasses()
	{
		AvailableClasses = await db.PublicationClasses
			.ToDropDown()
			.ToListAsync();
	}

	public class PublicationClassEditModel
	{
		public string Title { get; init; } = "";

		[Display(Name = "PublicationClass")]
		public int ClassId { get; init; }
	}
}
