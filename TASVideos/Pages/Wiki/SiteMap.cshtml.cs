using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.EditWikiPages)]
public class SiteMapModel : BasePageModel
{
	private readonly IWikiPages _wikiPages;

	private static readonly List<SiteMapEntry> CorePages = Assembly
		.GetAssembly(typeof(SiteMapModel))
		!.GetTypes()
		.Where(type => typeof(BasePageModel).IsAssignableFrom(type))
		.Where(type => type != typeof(BasePageModel))
		.Select(t => new SiteMapEntry
		{
			PageName = t.Namespace
				?.Replace("TASVideos.Pages.", "")
				.Replace(".", "/") + "/"
				+ t.Name.Replace("Model", ""),
			IsWiki = false,
			AccessRestriction = AccessRestriction(t)
		})
		.ToList();

	public SiteMapModel(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	public List<SiteMapEntry> Map { get; set; } = new();

	public void OnGet()
	{
		Map = CorePages.ToList();
		var wikiPages = _wikiPages.Query
			.ThatAreSubpagesOf("")
			.Where(w => !w.PageName.StartsWith("InternalSystem"))
			.Select(w => w.PageName)
			.ToList();

		Map.AddRange(wikiPages
			.Distinct()
			.Select(p => new SiteMapEntry
			{
				PageName = p,
				IsWiki = true,
				AccessRestriction = "Anonymous"
			}));
	}

	private static string AccessRestriction(MemberInfo type)
	{
		// This logic is far from robust and full of assumptions, the idea is to tweak as necessary
		if (type.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
		{
			return "Anonymous";
		}

		if (type.GetCustomAttribute<AuthorizeAttribute>() is not null)
		{
			return "Logged In";
		}

		if (type.GetCustomAttribute<RequireEdit>() is not null)
		{
			return "Any Wiki Editing";
		}

		var requiredPermAttr = type.GetCustomAttribute<RequirePermissionAttribute>();
		if (requiredPermAttr is not null)
		{
			return requiredPermAttr.MatchAny
				? string.Join(" or ", requiredPermAttr.RequiredPermissions)
				: string.Join(", ", requiredPermAttr.RequiredPermissions);
		}

		return "Unknown";
	}
}
