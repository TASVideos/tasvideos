using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Controllers;
using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Tasks;

namespace TASVideos.Pages.Wiki
{
	[RequirePermission(PermissionTo.SeeAdminPages)]
	public class SiteMapModel : BasePageModel
	{
		private readonly WikiTasks _wikiTasks;

		// TODO: add razor pages to this!
		private static readonly List<Models.SiteMapModel> CorePages = Assembly
			.GetAssembly(typeof(WikiController))
			.GetTypes()
			.Where(type => typeof(Controller).IsAssignableFrom(type))
			.SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
			.Where(m => !m.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Any())
			.Where(m => m.GetCustomAttribute<HttpPostAttribute>() == null)
			.Select(m => new Models.SiteMapModel
			{
				PageName = m.Name == "Index"
					? m.DeclaringType.Name.Replace("Controller", "")
					: $"{m.DeclaringType.Name.Replace("Controller", "")}/{m.Name}",
				IsWiki = false,
				AccessRestriction = AccessRestriction(m)
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

		private static string AccessRestriction(MethodInfo action)
		{
			// This logic is far from robust and full of assumptions, the idea is to tweak as necessary
			if (action.GetCustomAttribute<AllowAnonymousAttribute>() != null
				|| action.DeclaringType.GetCustomAttribute<AllowAnonymousAttribute>() != null)
			{
				return "Anonymous";
			}

			if (action.GetCustomAttribute<AuthorizeAttribute>() != null
				|| action.DeclaringType.GetCustomAttribute<AuthorizeAttribute>() != null)
			{
				return "Logged In";
			}

			if (action.GetCustomAttribute<RequireEditAttribute>() != null
				|| action.DeclaringType.GetCustomAttribute<RequireEditAttribute>() != null)
			{
				return "Edit Permissions";
			}

			var requiredPermAttr = action.GetCustomAttribute<Filter.RequirePermissionAttribute>()
				?? action.DeclaringType.GetCustomAttribute<Filter.RequirePermissionAttribute>();
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
