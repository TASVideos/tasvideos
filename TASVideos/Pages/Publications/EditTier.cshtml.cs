using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.SetTier)]
	public class EditTierModel : BasePageModel
	{
		private readonly PublicationTasks _publicationTasks;

		public EditTierModel(
			PublicationTasks publicationTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_publicationTasks = publicationTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public PublicationTierEditModel Publication { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Publication = await _publicationTasks.GetTiersForEdit(Id);
			if (Publication == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			var result = await _publicationTasks.UpdateTier(Id, Publication.TierId);
			if (result)
			{
				return RedirectToPage("View", new { Id });
			}

			return NotFound();
		}
	}
}
