using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class SystemPagesModel(IWikiPages wikiPages) : BasePageModel
{
	public List<SystemPage> SystemPages { get; set; } = [];

	public async Task OnGet()
	{
		foreach (var page in SystemWiki.Pages)
		{
			var exists = await wikiPages.Exists(page);
			SystemPages.Add(new SystemPage(page, exists));
		}
	}

	public record SystemPage(string Name, bool Exists);
}
