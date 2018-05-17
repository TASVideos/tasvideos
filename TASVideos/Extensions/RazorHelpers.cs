using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Data.Entity;
using TASVideos.ViewComponents;

namespace TASVideos.Extensions
{
    public static class RazorHelpers
    {
		public static bool UserHasPermission(this ViewDataDictionary viewData, PermissionTo permission)
		{
			return ((IEnumerable<PermissionTo>)viewData["UserPermissions"]).Contains(permission);
		}

		public static bool UserHasAnyPermission(this ViewDataDictionary viewData, IEnumerable<PermissionTo> permissions)
		{
			return permissions.Any(permission => ((IEnumerable<PermissionTo>)viewData["UserPermissions"]).Contains(permission));
		}

		public static string Controller(this ViewContext viewContext)
		{
			return viewContext.ActionDescriptor.RouteValues["controller"];
		}

		public static string Action(this ViewContext viewContext)
		{
			return viewContext.ActionDescriptor.RouteValues["action"];
		}

		public static async Task<IHtmlContent> RenderWiki(this IViewComponentHelper component, string pageName)
		{
			return await component.InvokeAsync(nameof(RenderWikiPage), new { url = pageName });
		}

		public static async Task<IHtmlContent> ListParents(this IViewComponentHelper component, WikiPage pageData)
		{
			return await component.InvokeAsync(nameof(ViewComponents.ListParents), new { pageData });
		}
	}
}
