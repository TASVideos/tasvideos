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
			switch (Activate)
			{
				case "Home" when ViewContext.PageGroup() == "Index":
				case "Forum" when ViewContext.PageGroup() == "Forum":
				case "Submissions" when ViewContext.PageGroup() == "Submissions":
				case "Movies" when ViewContext.PageGroup() == "Publications":
				case "Admin" when ViewContext.PageGroup() == "Roles":
				case "Admin" when ViewContext.PageGroup() == "Users":
				case "Admin" when ViewContext.PageGroup() == "Permissions":
					return true;
			}

			if (Activate == (string)ViewContext.ViewData["ActiveTab"])
			{
				return true;
			}

			if (Activate == (string)ViewContext.TempData["ActiveTab"])
			{
				return true;
			}

			// Wiki Razor Pages that are not the general wiki page action
			if (string.IsNullOrWhiteSpace((string)ViewContext.ViewData["ActiveTab"])
				&& string.IsNullOrWhiteSpace((string)ViewContext.TempData["ActiveTab"])
				&& Activate == "Wiki" && ViewContext.PageGroup() == "Wiki")
			{
				return true;
			}

			return false;
		}
	}
}
