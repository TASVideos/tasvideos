using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using TASVideos.Core.Services;
using TASVideos.Pages;
using TASVideos.Pages.Wiki;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.BrokenLinks)]
public class BrokenLinks : ViewComponent
{
	private record PageEntry(string Name, bool HasRoute);

	private static readonly List<PageEntry> CorePages = Assembly
		.GetAssembly(typeof(SiteMapModel))
		!.GetTypes()
		.Where(type => typeof(BasePageModel).IsAssignableFrom(type))
		.Where(type => type != typeof(BasePageModel))
		.Select(t => new PageEntry(
			t.Namespace?.Replace("TASVideos.Pages.", "")
					.Replace(".", "/") + "/"
				+ t.Name.Replace("Model", ""), t.GetProperties().Any(p => p.HasAttribute<FromRouteAttribute>())))
		.ToList();

	private readonly IWikiPages _wikiPages;

	public BrokenLinks(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

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

		// These should be updated one day, but there are far too many for now
		var tempRoutedExceptions = new[] { "forum/t", "forum/f", "forum/p" };

		// For pages with routes, assume anything added to the end is okay
		var routedPages = corePages
			.Where(c => c.HasRoute)
			.Select(c => c.Name.ToLowerInvariant())
			.Concat(tempRoutedExceptions)
			.ToList();

		var brokenLinks = await _wikiPages.BrokenLinks();

		var filtered = brokenLinks
			.Where(b => !generalPages.Contains(b.Referral.Split('?')[0].ToLowerInvariant()))
			.Where(b => !routedPages.Any(r => b.Referral.ToLowerInvariant().StartsWith(r)))
			.ToList();

		return View(filtered);
	}
}
