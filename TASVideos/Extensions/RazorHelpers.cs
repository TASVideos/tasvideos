using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Data.Entity;
using TASVideos.ViewComponents;

namespace TASVideos.Extensions
{
	public static class RazorHelpers
	{
		public static string ReturnUrl(this HttpContext context)
		{
			return $"{context.Request.Path}{context.Request.QueryString}";
		}

		public static bool UserHas(this ViewDataDictionary viewData, PermissionTo permission)
		{
			return ((IEnumerable<PermissionTo>)viewData["UserPermissions"]).Contains(permission);
		}

		public static bool UserHasAny(this ViewDataDictionary viewData, IEnumerable<PermissionTo> permissions)
		{
			return permissions.Any(permission => ((IEnumerable<PermissionTo>)viewData["UserPermissions"]).Contains(permission));
		}

		public static string Page(this ViewContext viewContext)
		{
			return viewContext.ActionDescriptor.DisplayName;
		}

		public static string PageGroup(this ViewContext viewContext)
		{
			return viewContext.ActionDescriptor.DisplayName
				?.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries)
				.FirstOrDefault();
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

		public static async Task<IHtmlContent> ListSubPages(this IViewComponentHelper component, WikiPage pageData, string pp = null)
		{
			return await component.InvokeAsync(nameof(ViewComponents.ListSubPages), new { pageData, pp });
		}

		public static async Task<IHtmlContent> HomePageHeader(this IViewComponentHelper component, WikiPage pageData)
		{
			return await component.InvokeAsync(nameof(ViewComponents.HomePageHeader), new { pageData });
		}

		public static async Task<IHtmlContent> HomePageFooter(this IViewComponentHelper component, WikiPage pageData)
		{
			return await component.InvokeAsync(nameof(ViewComponents.HomePageFooter), new { pageData });
		}

		public static async Task<IHtmlContent> GameResourcesHeader(this IViewComponentHelper component, WikiPage pageData)
		{
			return await component.InvokeAsync(nameof(ViewComponents.GameResourcesHeader), new { pageData });
		}

		public static async Task<IHtmlContent> GameResourcesFooter(this IViewComponentHelper component, WikiPage pageData)
		{
			return await component.InvokeAsync(nameof(ViewComponents.GameResourcesFooter), new { pageData });
		}

		public static async Task<IHtmlContent> SystemPageHeader(this IViewComponentHelper component, WikiPage pageData)
		{
			return await component.InvokeAsync(nameof(ViewComponents.SystemPageHeader), new { pageData });
		}

		public static async Task<IHtmlContent> SystemPageFooter(this IViewComponentHelper component, WikiPage pageData)
		{
			return await component.InvokeAsync(nameof(ViewComponents.SystemPageFooter), new { pageData });
		}

		public static string UniqueId(this ViewDataDictionary viewData)
		{
			return "_" + Guid.NewGuid().ToString().Replace("-", "").ToLower();
		}
	}
}
