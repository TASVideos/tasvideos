using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class CatalogModel : BasePageModel
	{
		private readonly PublicationTasks _publicationTasks;

		public CatalogModel(
			PublicationTasks publicationTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_publicationTasks = publicationTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		public PublicationCatalogModel Catalog { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Catalog = await _publicationTasks.Catalog(Id);
			if (Catalog == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page(); // TODO: repopulate dropdowns
			}

			await _publicationTasks.UpdateCatalog(Id, Catalog);
			return RedirectToPage("View", new { Id });
		}
	}
}
