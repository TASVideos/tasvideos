using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.EditPublicationMetaData)]
	public class EditModel : BasePageModel
	{
		private readonly PublicationTasks _publicationTasks;

		public EditModel(
			PublicationTasks publicationTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_publicationTasks = publicationTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public PublicationEditModel Publication { get; set; } = new PublicationEditModel();

		// TODO: Shim for now, fix two column picker to properly handle nested available lists
		public IEnumerable<SelectListItem> AvailableFlags => Publication.AvailableFlags;
		public IEnumerable<SelectListItem> AvailableTags => Publication.AvailableTags;

		public async Task<IActionResult> OnGet()
		{
			Publication = await _publicationTasks.GetPublicationForEdit(Id, UserPermissions);
			if (Publication == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				Publication.AvailableMoviesForObsoletedBy =
					await _publicationTasks.GetAvailableMoviesForObsoletedBy(Id, Publication.SystemCode);
				Publication.AvailableFlags = await _publicationTasks.GetAvailableFlags(UserPermissions);
				Publication.AvailableTags = await _publicationTasks.GetAvailableTags();

				return Page();
			}

			await _publicationTasks.UpdatePublication(Id, Publication);
			return RedirectToPage("View", new { Id });
		}
	}
}
