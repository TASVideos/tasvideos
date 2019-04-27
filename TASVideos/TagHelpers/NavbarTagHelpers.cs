using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.Extensions;

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
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "li";
			output.AddCssClass("nav-item");
			if (IsActive())
			{
				output.AddCssClass("active");
			}
		}
	}

	public class NavDropdownTagHelper : NavItemBase
	{
		public string Name { get; set; }

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var content = (await output.GetChildContentAsync()).GetContent();

			output.TagName = "li";
			output.AddCssClass("nav-item dropdown");
			if (IsActive())
			{
				output.AddCssClass("active");
			}

			if (string.IsNullOrWhiteSpace(Name))
			{
				Name = Activate;
			}

			output.Content.SetHtmlContent(
				$"<a href='#' class='nav-link dropdown-toggle' data-toggle='dropdown'>{Name}<span class='caret'></span></a>");

			output.Content.AppendHtml($"<div class='dropdown-menu'>{content}</div>");
		}
	}

	public class NavItemBase : TagHelper
	{
		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public string Activate { get; set; }

		protected bool IsActive()
		{
			if (string.IsNullOrWhiteSpace(Activate))
			{
				return false;
			}

			var viewActiveTab = (string)ViewContext.ViewData["ActiveTab"];
			if (Activate == viewActiveTab)
			{
				return true;
			}

			var tempActiveTab = (string)ViewContext.TempData["ActiveTab"];
			if (Activate == tempActiveTab)
			{
				return true;
			}

			var page = ViewContext.Page();
			var pageGroup = ViewContext.PageGroup();

			// We don't want to activate the wiki menu on every wiki page
			if (Activate == pageGroup && pageGroup != "Wiki")
			{
				return true;
			}

			switch (Activate)
			{
				case "Home" when pageGroup == "Home" || new[] { "WelcomeToTASVideos", "News" }.Contains(viewActiveTab):
				case "Movies" when pageGroup == "Publications":
				case "Admin" when pageGroup == "Roles":
				case "Admin" when pageGroup == "Users":
				case "Admin" when pageGroup == "Permissions":
				case "Register" when page == "/Account/Register":
				case "Login" when page == "/Account/Login":
					return true;
				case "Wiki" when new[] { "SandBox", "RecentChanges", "WikiOrphans", "TODO", "System", "DeletedPages" }.Contains(viewActiveTab):
					return true;
			}

			// Wiki Razor Pages that are not the general wiki page action
			if (string.IsNullOrWhiteSpace(viewActiveTab)
				&& string.IsNullOrWhiteSpace(tempActiveTab)
				&& Activate == "Wiki" && pageGroup == "Wiki")
			{
				return true;
			}

			return false;
		}
	}
}
