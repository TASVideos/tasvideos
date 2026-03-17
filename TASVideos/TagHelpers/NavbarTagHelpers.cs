using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

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
		var addClass = "nav-link dropdown-toggle";
		if (IsActive())
		{
			addClass += " active";
		}

		if (string.IsNullOrWhiteSpace(Name))
		{
			Name = Activate;
		}

		output.Content.SetHtmlContent(
			$"<a href='#' class='{addClass}' data-bs-toggle='dropdown'>{Name ?? ""}</a>");

		output.Content.AppendHtml($"<div class='dropdown-menu'>{content}</div>");
	}
}

public class NavItemBase : TagHelper
{
	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public string? Activate { get; set; }

	protected bool IsActive()
	{
		if (string.IsNullOrWhiteSpace(Activate))
		{
			return false;
		}

		var viewPaths = ((string?)ViewContext.TempData["ActiveTab"] ?? "").SplitWithEmpty("/");

		// If length of the name is 2, assume it is the language prefix and use the next part of the path for tab matching
		var viewActiveTab = viewPaths.Length > 1 && viewPaths[0].Length == 2
			? viewPaths[1]
			: viewPaths.FirstOrDefault();

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
			case "Movies" when MoviesGroup.Contains(pageGroup) || MoviesWikiGroup.Contains(viewActiveTab):
			case "Games" when GamesWikiGroup.Contains(viewActiveTab):
			case "Tools" when ToolsWikiGroup.Contains(viewActiveTab):
			case "Chat" when ChatWikiGroup.Contains(viewActiveTab):
			case "Resources" when ResourcesWikiGroup.Contains(viewActiveTab):
			case "About" when AboutWikiGroup.Contains(viewActiveTab):
			case "Wiki" when WikiGroup.Contains(viewActiveTab):
			case "Admin" when AdminGroup.Contains(pageGroup) || AdminWikiGroup.Contains(viewActiveTab):
			case "Register" when page == "/Account/Register":
			case "Login" when page == "/Account/Login":
				return true;
		}

		// Wiki Razor Pages that are not the general wiki page action
		return string.IsNullOrWhiteSpace(viewActiveTab)
			&& Activate == "Wiki" && pageGroup == "Wiki";
	}

	private static readonly string[] MoviesGroup = ["Publications", "UserFiles"];
	private static readonly string[] MoviesWikiGroup = ["Class"];
	private static readonly string[] GamesWikiGroup = ["GameResources", "Platforms"];
	private static readonly string[] ToolsWikiGroup = ["Bizhawk", "EmulatorResources"];
	private static readonly string[] ChatWikiGroup = ["LiveChat", "Communities"];
	private static readonly string[] ResourcesWikiGroup = ["ArticleIndex", "FAQ", "TasingGuide"];
	private static readonly string[] AboutWikiGroup = ["WelcomeToTASVideos", "Staff", "Helping", "SiteHistory"];
	private static readonly string[] WikiGroup = ["SandBox", "RecentChanges", "WikiOrphans", "TODO", "System", "DeletedPages"];
	private static readonly string[] AdminGroup = ["Roles", "Users", "Permissions", "Tags", "Flags", "Genres", "PublicationClasses", "AwardsEditor", "Systems"];
	private static readonly string[] AdminWikiGroup = ["MediaPosts"];
}
