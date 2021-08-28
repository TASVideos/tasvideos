using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tiers
{
	[RequirePermission(PermissionTo.TierMaintenance)]
	public class EditModel : BasePageModel
	{
		private readonly ITierService _tierService;

		public EditModel(ITierService tierService)
		{
			_tierService = tierService;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public Tier Tier { get; set; } = new ();
		public bool InUse { get; set; } = true;

		public async Task<IActionResult> OnGet()
		{
			var tier = await _tierService.GetById(Id);
			if (tier == null)
			{
				return NotFound();
			}

			Tier = tier;
			InUse = await _tierService.InUse(Id);
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var result = await _tierService.Edit(Id, Tier);
			switch (result)
			{
				default:
				case TierEditResult.Success:
					SuccessStatusMessage("Tag successfully updated.");
					return BasePageRedirect("Index");
				case TierEditResult.NotFound:
					return NotFound();
				case TierEditResult.DuplicateName:
					ModelState.AddModelError($"{nameof(Tier)}.{nameof(Tier.Name)}", $"{nameof(Tier.Name)} {Tier.Name} already exists");
					ClearStatusMessage();
					return Page();
				case TierEditResult.Fail:
					ErrorStatusMessage($"Unable to delete Tag {Id}, the tag may have already been deleted or updated.");
					return Page();
			}
		}

		public async Task<IActionResult> OnPostDelete()
		{
			var result = await _tierService.Delete(Id);
			switch (result)
			{
				case TierDeleteResult.InUse:
					ErrorStatusMessage($"Unable to delete Tier {Id}, the tier is in use by at least 1 publication.");
					break;
				case TierDeleteResult.Success:
					SuccessStatusMessage($"Tier {Id}, deleted successfully.");
					break;
				case TierDeleteResult.NotFound:
					ErrorStatusMessage($"Tier {Id}, not found.");
					break;
				case TierDeleteResult.Fail:
					ErrorStatusMessage($"Unable to delete Tier {Id}, the tier may have already been deleted or updated.");
					break;
			}

			return BasePageRedirect("Index");
		}
	}
}
