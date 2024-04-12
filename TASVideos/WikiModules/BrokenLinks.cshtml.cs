using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen;
using TASVideos.Core.Services.Wiki;
using TASVideos.Pages;
using TASVideos.Pages.Wiki;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.BrokenLinks)]
public class BrokenLinks(IWikiPages wikiPages) : WikiViewComponent
{
	private static readonly List<PageEntry> CorePages = Assembly
		.GetAssembly(typeof(SiteMapModel))
		!.GetTypes()
		.Where(type => typeof(BasePageModel).IsAssignableFrom(type))
		.Where(type => type != typeof(BasePageModel))
		.Select(t => new PageEntry(
			t.Namespace?.Replace("TASVideos.Pages.", "")
					.Replace(".", "/") + "/"
				+ t.Name.Replace("Model", ""),
			t.GetProperties().Any(p => p.HasAttribute<FromRouteAttribute>())))
		.ToList();

	public List<WikiPageReferral> Links { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var indexTrimmed = CorePages
			.Where(p => p.Name.Contains("/Index"))
			.Select(p => p with { Name = p.Name.Replace("/Index", "") });

		var corePages = CorePages
			.Concat(indexTrimmed)
			.ToList();

		// These are okay
		var generalExceptions = new[] { "frontpage", "api" };

		var generalPages = corePages
			.Where(c => !c.HasRoute)
			.Select(c => c.Name.ToLowerInvariant())
			.Concat(generalExceptions)
			.ToList();

		var rssFeeds = generalPages
			.Where(p => p.StartsWith("rssfeeds/"))
			.Select(p => p.Replace("rssfeeds/", "") + ".rss")
			.ToList();

		generalPages = [.. generalPages, .. rssFeeds];

		// For pages with routes, assume anything added to the end is okay
		var routedPages = corePages
			.Where(c => c.HasRoute)
			.Select(c => c.Name.ToLowerInvariant())
			.ToList();

		var brokenLinks = await wikiPages.BrokenLinks();

		var filtered = brokenLinks
			.Where(b => !generalPages.Contains(b.Referral.Split('?')[0].ToLowerInvariant()))
			.Where(b => !routedPages.Any(r => b.Referral.StartsWith(r, StringComparison.InvariantCultureIgnoreCase)))
			.ToList();

		Links = await FilterRevisionLinks(filtered);
		return View();
	}

	// Hack for the inability to strip query strings from wiki page referrals
	// This assumes there are not many of these, and so the performance hit is minor!
	private async Task<List<WikiPageReferral>> FilterRevisionLinks(IReadOnlyCollection<WikiPageReferral> filtered)
	{
		var revisionLinks = filtered
			.Where(b => b.Referral.Contains("?revision"))
			.ToList();
		var existingRevisionLinks = new List<string>();
		foreach (var link in revisionLinks)
		{
			var page = link.Referral.Split('?')[0];
			if (await wikiPages.Exists(page))
			{
				existingRevisionLinks.Add(link.Referral);
			}
		}

		return filtered
			.Where(f => !existingRevisionLinks.Contains(f.Referral))
			.ToList();
	}

	private record PageEntry(string Name, bool HasRoute);
}
