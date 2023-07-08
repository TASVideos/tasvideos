using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class SystemPagesModel : BasePageModel
{
	private readonly IWikiPages _wikiPages;

	public SystemPagesModel(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	public record SystemPage(string Name, bool Exists);
	public List<SystemPage> SystemPages { get; set; } = new List<SystemPage>();

	public async Task<IActionResult> OnGet()
	{
		foreach (var page in SystemWiki.Pages)
		{
			var exists = await _wikiPages.Exists(page);
			SystemPages.Add(new SystemPage(page, exists));
		}

		return Page();
	}
}
