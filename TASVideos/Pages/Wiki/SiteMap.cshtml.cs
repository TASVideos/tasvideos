using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Authorization;

using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Pages.Wiki
{
	[RequirePermission(PermissionTo.SeeAdminPages)]
	public class SiteMapModel : BasePageModel
	{
		private readonly WikiTasks _wikiTasks;

		private static readonly List<Models.SiteMapModel> CorePages = Assembly
			.GetAssembly(typeof(SiteMapModel))
			.GetTypes()
			.Where(type => typeof(BasePageModel).IsAssignableFrom(type))
			.Where(type => type != typeof(BasePageModel))
			.Select(t => new Models.SiteMapModel
			{
				PageName = t.Namespace
					.Replace("TASVideos.Pages.", "")
					.Replace(".", "/") + "/"
					+ t.Name.Replace("Model", ""),
				IsWiki = false,
				AccessRestriction = AccessRestriction(t)
			}) 
			.ToList();

		public SiteMapModel(
			WikiTasks wikiTasks,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_wikiTasks = wikiTasks;
		}

		// TODO: rename this model
		public List<Models.SiteMapModel> Map { get; set; } = new List<Models.SiteMapModel>();

		public void OnGet()
		{
			Map = CorePages.ToList();
			var wikiPages = _wikiTasks.GetSubPages("");
			Map.AddRange(wikiPages
				.Distinct()
				.Select(p => new Models.SiteMapModel
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
