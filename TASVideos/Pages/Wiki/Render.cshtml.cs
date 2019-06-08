using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Services;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class RenderModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;
		private readonly ApplicationDbContext _db;

		public RenderModel(IWikiPages wikiPages, ApplicationDbContext db)
		{
			_wikiPages = wikiPages;
			_db = db;
		}

		public string Markup { get; set; }

		public WikiPage WikiPage { get; set; }

		public async Task<IActionResult> OnGet(string url, int? revision = null)
		{
			url = (url ?? "").Trim('/');
			if (url?.ToLower() == "frontpage")
			{
				return Redirect("/");
			}

			if (WikiHelper.IsHomePage(url))
			{
				if (!await UserNameExists(url))
				{
					return RedirectToPage("/Wiki/HomePageDoesNotExist");
				}
			}

			if (!WikiHelper.IsValidWikiPageName(url))
			{
				// Support legacy links like [adelikat] that should have been [user:adelikat]
				if (await _wikiPages.Exists("HomePages/" + url))
				{
					return Redirect("HomePages/" + url);
				}

				return RedirectToPage("/Wiki/PageNotFound", new { possibleUrl = WikiEngine.Builtins.NormalizeInternalLink(url) });
			}

			WikiPage = await _wikiPages.Page(url, revision);

			if (WikiPage != null)
			{
				ViewData["WikiPage"] = WikiPage;
				ViewData["Title"] = WikiPage.PageName;
				Markup = WikiPage.Markup;
				return Page();
			}

			var homePage = await _wikiPages.Page("HomePages/" + url);
			if (homePage != null)
			{
				// We redirected on invalid url homepages, now we have to do the same for valid ones
				return Redirect("HomePages/" + url);
			}

			// Account for garbage revision values
			if (revision.HasValue && await _wikiPages.Exists(url)) 
			{
				return Redirect("/" + url);
			}

			return RedirectToPage("/Wiki/PageDoesNotExist", new { url });
		}

		private async Task<bool> UserNameExists(string path)
		{
			var userName = WikiHelper.ToUserName(path);
			return await _db.Users.Exists(userName);
		}
	}
}
