using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.Extensions;
using static TASVideos.TagHelpers.TagHelperExtensions;

namespace TASVideos.TagHelpers
{
	public class NavbarTagHelper : TagHelper
	{
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "ul";
			output.AddCssClass("navbar-nav");
		}
	}

	public class NavItemTagHelper : NavItemBase
	{
		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var content = (await output.GetChildContentAsync()).GetContent();

			output.TagName = "li";
			output.AddCssClass("nav-item");
			if (IsActive())
			{
				content = content.Replace("nav-link", "nav-link active");
			}

			output.Content.SetHtmlContent(content);
		}
	}

	public class NavDropdownTagHelper : NavItemBase
	{
		public string? Name { get; set; }

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var content = (await output.GetChildContentAsync()).GetContent();

			output.TagName = "li";
			output.AddCssClass("nav-item dropdown");
			string addClass = "nav-link dropdown-toggle";
			if (IsActive())
			{
				addClass += " active";
			}

			if (string.IsNullOrWhiteSpace(Name))
			{
				Name = Activate;
			}

			output.Content.SetHtmlContent(
				$"<a href='#' class='{addClass}' data-bs-toggle='dropdown'>{Name ?? ""}<span class='caret'></span></a>");

			output.Content.AppendHtml($"<div class='dropdown-menu'>{content}</div>");
		}
	}

	public class NavItemBase : TagHelper
	{
		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; } = new ();

		public string? Activate { get; set; }

		protected bool IsActive()
		{
			if (string.IsNullOrWhiteSpace(Activate))
			{
				return false;
			}

			var viewPaths = ((string?)ViewContext.ViewData["ActiveTab"] ?? "").SplitWithEmpty("/");

			// If length of the name is 2, assume it is the language prefix and use the next part of the path for tab matching
			var viewActiveTab = viewPaths.Length > 1 && viewPaths[0].Length == 2
				? viewPaths[1]
				: viewPaths.FirstOrDefault();

			var tempActiveTab = (string?)ViewContext.TempData["ActiveTab"];
			if (!string.IsNullOrWhiteSpace(tempActiveTab))
			{
				viewActiveTab = tempActiveTab;
			}

			var page = ViewContext.Page();
			var pageGroup = ViewContext.PageGroup();

			// We don't want to activate the wiki menu on every wiki page
			if (Activate == pageGroup && pageGroup != "Wiki")
			{
				return true;
			}

			if (Activate == viewActiveTab)
			{
				return true;
			}

			switch (Activate)
			{
				case "Home" when page == "/Index":
				case "Movies" when new[] { "Publications", "Submissions", "UserFiles" }.Contains(pageGroup):
				case "Articles" when new[] { "ArticleIndex", "Game Resources", "EmulatorResources" }.Contains(viewActiveTab):
				case "Admin" when new[] { "Roles", "Users", "Permissions" }.Contains(pageGroup):
				case "Register" when page == "/Account/Register":
				case "Login" when page == "/Account/Login":
				case "Wiki" when new[] { "SandBox", "RecentChanges", "WikiOrphans", "TODO", "System", "DeletedPages" }.Contains(viewActiveTab):
					return true;
			}

			// Wiki Razor Pages that are not the general wiki page action
			if (string.IsNullOrWhiteSpace(viewActiveTab)
				&& Activate == "Wiki" && pageGroup == "Wiki")
			{
				return true;
			}

			return false;
		}
	}
}
