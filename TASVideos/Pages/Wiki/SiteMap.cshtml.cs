using System.Reflection;

namespace TASVideos.Pages.Wiki;

[RequirePermission(PermissionTo.EditWikiPages)]
public class SiteMapModel(ApplicationDbContext db) : BasePageModel
{
	private static readonly List<SiteMapEntry> CorePages = Assembly
		.GetAssembly(typeof(SiteMapModel))
		!.GetTypes()
		.Where(type => typeof(BasePageModel).IsAssignableFrom(type))
		.Where(type => type != typeof(BasePageModel))
		.Select(t => new SiteMapEntry(
			t.Namespace
				?.Replace("TASVideos.Pages.", "")
				.Replace(".", "/") + "/"
				+ t.Name.Replace("Model", ""),
			false,
			AccessRestriction(t)))
		.ToList();

	public List<SiteMapEntry> Map { get; set; } = [];

	public void OnGet()
	{
		var wikiPages = db.WikiPages
			.ThatAreSubpagesOf("")
			.Where(w => !w.PageName.StartsWith("InternalSystem"))
			.Select(w => w.PageName)
			.ToList();

		Map = CorePages.Concat(wikiPages
			.Distinct()
			.Select(p => new SiteMapEntry(p, true, "Anonymous")))
			.ToList();
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

	public record SiteMapEntry(string PageName, bool IsWiki, string AccessRestriction);
}
