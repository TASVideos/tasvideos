using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class RenderModel(IWikiPages wikiPages, ApplicationDbContext db, ILogger<RenderModel> logger)
	: BasePageModel
{
	public IWikiPage WikiPage { get; set; } = null!;

	public async Task<IActionResult> OnGet(string? url, int? revision = null)
	{
		url = WebUtility.UrlDecode(url?.Trim('/') ?? "");
		if (url.ToLower() == "frontpage")
		{
			return Redirect("/");
		}

		if (WikiHelper.IsHomePage(url))
		{
			if (!await UserNameExists(url) && !await wikiPages.Exists(url))
			{
				return RedirectToPage("/Wiki/HomePageDoesNotExist");
			}
		}

		if (!WikiHelper.IsValidWikiPageName(url))
		{
			// Support legacy links like [adelikat] that should have been [user:adelikat]
			if (await wikiPages.Exists(LinkConstants.HomePages + url))
			{
				return Redirect(LinkConstants.HomePages + url);
			}

			return RedirectToPage("/Wiki/PageNotFound", new { possibleUrl = WikiEngine.Builtins.NormalizeInternalLink(url) });
		}

		var wikiPage = await wikiPages.Page(url, revision);
		if (wikiPage is not null)
		{
			if (logger.IsEnabled(LogLevel.Information))
			{
				logger.LogInformation("Rendering WikiPage {wikiPage}", wikiPage.PageName);
			}

			WikiPage = wikiPage;
			ViewData.SetWikiPage(WikiPage);
			ViewData.SetTitle(WikiPage.PageName);
			return Page();
		}

		// Account for garbage revision values
		if (revision.HasValue && await wikiPages.Exists(url))
		{
			return Redirect("/" + url);
		}

		return RedirectToPage("/Wiki/PageDoesNotExist", new { url });
	}

	private async Task<bool> UserNameExists(string path)
	{
		var userName = WikiHelper.ToUserName(path);
		return await db.Users.Exists(userName);
	}
}
