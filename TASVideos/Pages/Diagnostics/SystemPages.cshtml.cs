using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class SystemPagesModel(IWikiPages wikiPages) : BasePageModel
{
	public record SystemPage(string Name, bool Exists);
	public List<SystemPage> SystemPages { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		foreach (var page in SystemWiki.Pages)
		{
			var exists = await wikiPages.Exists(page);
			SystemPages.Add(new SystemPage(page, exists));
		}

		return Page();
	}
}
