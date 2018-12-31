using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Filter;
using TASVideos.Razor;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class WikiController : BaseController
	{
		private readonly IWikiPages _wikiPages;
		private readonly WikiMarkupFileProvider _wikiMarkupFileProvider;
		private readonly ExternalMediaPublisher _publisher;

		public WikiController(
			UserTasks userTasks,
			IWikiPages wikiPages,
			WikiMarkupFileProvider wikiMarkupFileProvider,
			ExternalMediaPublisher publisher)
			: base(userTasks)
		{
			_wikiPages = wikiPages;
			_wikiMarkupFileProvider = wikiMarkupFileProvider;
			_publisher = publisher;
		}

		[AllowAnonymous]
		public IActionResult RenderWikiPage(string url, int? revision = null)
		{
			url = url.Trim('/').Replace(".html", "");

			if (url.ToLower() == "frontpage")
			{
				return Redirect("/");
			}

			if (!WikiHelper.IsValidWikiPageName(url))
			{
				return RedirectToPage("/Wiki/PageNotFound", new { possibleUrl = WikiHelper.NormalizeWikiPageName(url) });
			}

			var existingPage = _wikiPages.Page(url, revision);

			if (existingPage != null)
			{
				ViewData["WikiPage"] = existingPage;
				ViewData["Title"] = existingPage.PageName;
				ViewData["Layout"] = "_WikiLayout";
				_wikiMarkupFileProvider.WikiPages = _wikiPages;
				return View(WikiMarkupFileProvider.Prefix + existingPage.Id, existingPage);
			}

			// Account for garbage revision values
			if (revision.HasValue && _wikiPages.Exists(url)) 
			{
				return Redirect("/" + url);
			}

			return RedirectToPage("/Wiki/PageDoesNotExist", new { url });
		}

		[AllowAnonymous]
		[HttpPost]
		public ViewResult GeneratePreview()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();

			ViewData["WikiPage"] = null;
			ViewData["Title"] = "Generated Preview";
			ViewData["Layout"] = null;
			var name = _wikiMarkupFileProvider.SetPreviewMarkup(input);

			return View(name);
		}

		[RequirePermission(PermissionTo.DeleteWikiPages)]
		public async Task<IActionResult> DeletePage(string path)
		{
			if (!string.IsNullOrWhiteSpace(path))
			{
				var result = await _wikiPages.Delete(path.Trim('/'));

				_publisher.SendGeneralWiki(
					$"Page {path} DELETED by {User.Identity.Name}",
					$"({result} revisions)",
					"");
			}

			return RedirectToPage("/Wiki/DeletedPages");
		}

		[RequirePermission(PermissionTo.DeleteWikiPages)]
		public async Task<IActionResult> DeleteRevision(string path, int revision)
		{
			if (string.IsNullOrWhiteSpace(path) || revision == 0)
			{
				return RedirectHome();
			}

			path = path.Trim('/');
			await _wikiPages.Delete(path, revision);

			_publisher.SendGeneralWiki(
					$"Revision {revision} of Page {path} DELETED by {User.Identity.Name}",
					"",
					"");

			return Redirect("/" + path);
		}

		[RequirePermission(PermissionTo.DeleteWikiPages)]
		public async Task<IActionResult> Undelete(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return RedirectHome();
			}

			path = path.Trim('/');
			await _wikiPages.Undelete(path);

			_publisher.SendGeneralWiki(
					$"Page {path} UNDELETED by {User.Identity.Name}",
					"",
					$"{BaseUrl}/path");

			return Redirect("/" + path);
		}
	}
}
