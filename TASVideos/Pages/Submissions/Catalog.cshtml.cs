using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(PermissionTo.CatalogMovies)]
	public class CatalogModel : BasePageModel
	{
		private readonly SubmissionTasks _submissionTasks;

		public CatalogModel(
			SubmissionTasks submissionTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_submissionTasks = submissionTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public SubmissionCatalogModel Catalog { get; set; } = new SubmissionCatalogModel();

		public async Task<IActionResult> OnGet()
		{
			Catalog = await _submissionTasks.Catalog(Id);
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
				await _submissionTasks.PopulateCatalogDropDowns(Catalog);
				return Page();
			}

			await _submissionTasks.UpdateCatalog(Id, Catalog);
			return RedirectToPage("View", new { Id });
		}
	}
}
