using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tiers
{
	[RequirePermission(PermissionTo.TierMaintenance)]
	public class CreateModel : BasePageModel
	{
		private readonly ITierService _tierService;

		public CreateModel(ITierService tierService)
		{
			_tierService = tierService;
		}

		[BindProperty]
		public Tier Tier { get; set; } = new ();

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var (_, result) = await _tierService.Add(Tier);
			switch (result)
			{
				default:
				case TierEditResult.Success:
					SuccessStatusMessage("Tier successfully created.");
					return RedirectToPage("Index");
				case TierEditResult.DuplicateName:
					ModelState.AddModelError($"{nameof(Tier)}.{nameof(Tier.Name)}", $"{nameof(Tier.Name)} {Tier.Name} already exists");
					ClearStatusMessage();
					return Page();
				case TierEditResult.Fail:
					ErrorStatusMessage("Unable to edit tier due to an unknown error");
					return Page();
			}
		}
	}
}
