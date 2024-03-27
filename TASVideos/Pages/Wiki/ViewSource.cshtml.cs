using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class ViewSourceModel(IWikiPages wikiPages) : BasePageModel
{
	[FromQuery]
	public string? Path { get; set; }

	[FromQuery]
	public int? Revision { get; set; }

	public IWikiPage WikiPage { get; set; } = null!;

	public async Task<IActionResult> OnGet()
	{
		Path = Path?.Trim('/') ?? "";
		var wikiPage = await wikiPages.Page(Path, Revision);

		if (wikiPage is null)
		{
			return NotFound();
		}

		WikiPage = wikiPage;
		return Page();
	}
}
