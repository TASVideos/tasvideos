using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Wiki
{
	[RequirePermission(PermissionTo.MoveWikiPages)]
	public class MoveModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;
		private readonly ExternalMediaPublisher _publisher;

		public MoveModel(
			IWikiPages wikiPages,
			ExternalMediaPublisher publisher,
			UserManager userManager)
			: base(userManager)
		{
			_wikiPages = wikiPages;
			_publisher = publisher;
		}

		[FromQuery]
		public string Path { get; set; }

		[BindProperty]
		public WikiMoveModel Move { get; set; } = new WikiMoveModel();

		public IActionResult OnGet()
		{
			if (!string.IsNullOrWhiteSpace(Path))
			{
				Path = Path.Trim('/');
				if (_wikiPages.Exists(Path))
				{
					Move = new WikiMoveModel
					{
						OriginalPageName = Path,
						DestinationPageName = Path
					};
					return Page();
				}
			}

			return NotFound();
		}

		public async Task<IActionResult> OnPost()
		{
			Move.OriginalPageName = Move?.OriginalPageName.Trim('/');
			Move.DestinationPageName = Move?.DestinationPageName.Trim('/');

			if (_wikiPages.Exists(Move.DestinationPageName, includeDeleted: true))
			{
				ModelState.AddModelError("Move.DestinationPageName", "The destination page already exists.");
			}

			if (!ModelState.IsValid)
			{
				return Page();
			}

			await _wikiPages.Move(Move.OriginalPageName, Move.DestinationPageName);

			_publisher.SendGeneralWiki(
						$"Page {Move.OriginalPageName} moved to {Move.DestinationPageName} by {User.Identity.Name}",
						"",
						$"{BaseUrl}/{Move.DestinationPageName}");

			return Redirect("/" + Move.DestinationPageName);
		}
	}
}
