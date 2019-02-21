using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Authorization;

using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Wiki
{
	[RequirePermission(PermissionTo.SeeAdminPages)]
	public class SiteMapModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;

		private static readonly List<SiteMapEntry> CorePages = Assembly
			.GetAssembly(typeof(SiteMapModel))
			.GetTypes()
			.Where(type => typeof(BasePageModel).IsAssignableFrom(type))
			.Where(type => type != typeof(BasePageModel))
			.Select(t => new SiteMapEntry
			{
				PageName = t.Namespace
					.Replace("TASVideos.Pages.", "")
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

		public List<SiteMapEntry> Map { get; set; } = new List<SiteMapEntry>();

		public void OnGet()
		{
			Map = CorePages.ToList();
			var wikiPages = _wikiPages
				.ThatAreSubpagesOf("")
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

		private static string AccessRestriction(Type type)
		{
			// This logic is far from robust and full of assumptions, the idea is to tweak as necessary
			if (type.GetCustomAttribute<AllowAnonymousAttribute>() != null)
			{
				return "Anonymous";
			}

			if (type.GetCustomAttribute<AuthorizeAttribute>() != null)
			{
				return "Logged In";
			}

			if (type.GetCustomAttribute<RequireEdit>() != null)
			{
				return "Any Wiki Editing";
			}

			var requiredPermAttr = type.GetCustomAttribute<RequirePermissionAttribute>();
			if (requiredPermAttr != null)
			{
				return requiredPermAttr.MatchAny
					? string.Join(" or ", requiredPermAttr.RequiredPermissions)
					: string.Join(", ", requiredPermAttr.RequiredPermissions);
			}

			return "Unknown";
		}
	}
}
