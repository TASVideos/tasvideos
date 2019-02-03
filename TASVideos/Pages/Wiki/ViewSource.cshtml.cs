using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class ViewSourceModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;

		public ViewSourceModel(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		[FromQuery]
		public string Path { get; set; }

		[FromQuery]
		public int? Revision { get; set; }

		public WikiPage WikiPage { get; set; }

		public IActionResult OnGet()
		{
			WikiPage = _wikiPages.Page(Path, Revision);

			if (WikiPage != null)
			{
				return Page();
			}

			return NotFound();
		}
	}
}
