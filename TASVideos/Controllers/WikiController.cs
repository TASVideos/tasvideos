using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;
using TASVideos.WikiEngine;

namespace TASVideos.Controllers
{
	// TODO: edit permissions
	public class WikiController : BaseController
	{
		private readonly WikiTasks _wikiTasks;

		public WikiController(
			UserTasks userTasks,
			WikiTasks wikiTasks)
			: base(userTasks)
		{
			_wikiTasks = wikiTasks;
		}

		[RequireEdit]
		public async Task<IActionResult> Edit(string path)
		{
			path = path?.Trim('/');
			if (! WikiHelper.IsValidWikiPageName(path))
			{
				return RedirectHome();
			}

			var existingPage = await _wikiTasks.GetPage(path);

			var model = new WikiEditModel
			{
				PageName = path,
				Markup = existingPage?.Markup ?? ""
			};

			return View(model);
		}

		[RequireEdit]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(WikiEditModel model)
		{
			if (ModelState.IsValid)
			{
				model.Referrals = Util.GetAllWikiLinks(model.Markup)
					.Select(l => new WikiReferralModel
					{
						Link = l,
						Excerpt = "TODO we need an except here"
					});

				await _wikiTasks.SavePage(model);
				return Redirect("/" + model.PageName.Trim('/'));
			}

			return View(model);
		}

		[RequireEdit]
		[HttpPost]
		public ContentResult GeneratePreview()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			var w = new StringWriter();
			Util.DebugWriteHtml(input, w);
			return Content(w.ToString(), "text/plain");
		}

		[AllowAnonymous]
		public IActionResult PageNotFound(string possibleUrl)
		{
			ViewData["possibleUrl"] = possibleUrl;
			return View();
		}

		[AllowAnonymous]
		public IActionResult PageDoesNotExist(string url)
		{
			ViewData["Title"] = url?.Trim('/');
			return View();
		}

		[AllowAnonymous]
		public async Task<IActionResult> RenderWikiPage(string url, int? revision = null)
		{
			url = url.Trim('/');
			if (!WikiHelper.IsValidWikiPageName(url))
			{
				return RedirectToAction(nameof(PageNotFound), new { possibleUrl = WikiHelper.TryConvertToValidPageName(url) });
			}

			var existingPage = await _wikiTasks.GetPage(url, revision);

			if (existingPage != null)
			{
				ViewData["WikiPage"] = existingPage;
				ViewData["Title"] = existingPage.PageName;
				return View(Razor.WikiMarkupFileProvider.Prefix + existingPage.Id, existingPage);
			}

			return RedirectToAction(nameof(PageDoesNotExist), new { url });
		}

		[AllowAnonymous]
		public async Task<IActionResult> ViewSource(string path, int? revision = null)
		{
			var existingPage = await _wikiTasks.GetPage(path, revision);

			if (existingPage != null)
			{
				return View(existingPage);
			}

			return RedirectHome();
		}

		[AllowAnonymous]
		public async Task<IActionResult> PageHistory(string path)
		{
			var model = await _wikiTasks.GetPageHistory(path);
			return View(model);
		}

		[RequirePermission(PermissionTo.MoveWikiPages)]
		public async Task<IActionResult> MovePage(string path)
		{
			if (!string.IsNullOrWhiteSpace(path))
			{
				path = path.Trim('/');
				if (await _wikiTasks.PageExists(path))
				{
					return View(new WikiMoveModel
					{
						OriginalPageName = path,
						DestinationPageName = path
					});
				}
			}

			return RedirectHome();
		}

		[HttpPost]
		[RequirePermission(PermissionTo.MoveWikiPages)]
		public async Task<IActionResult> MovePage(WikiMoveModel model)
		{
			model.OriginalPageName = model.OriginalPageName.Trim('/');
			model.DestinationPageName = model.DestinationPageName.Trim('/');

			if (await _wikiTasks.PageExists(model.DestinationPageName))
			{
				ModelState.AddModelError(nameof(WikiMoveModel.DestinationPageName), "The destination page already exists.");
			}

			if (ModelState.IsValid)
			{
				await _wikiTasks.MovePage(model);
				return Redirect("/" + model.DestinationPageName);
			}

			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Referrers(string path)
		{
			if (!string.IsNullOrWhiteSpace(path))
			{
				path = path.Trim('/');
				var referrers = await _wikiTasks.GetReferrers(path);
				ViewData["PageName"] = path;

				return View(referrers);
			}

			return RedirectHome();
		}

		[AllowAnonymous]
		public async Task<IActionResult> Diff(string path, int? fromRevision, int? toRevision)
		{
			path = path.Trim('/');

			var model = fromRevision.HasValue && toRevision.HasValue
				? await _wikiTasks.GetPageDiff(path, fromRevision.Value, toRevision.Value)
				: await _wikiTasks.GetLatestPageDiff(path);

			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> DiffData(string path, int fromRevision, int toRevision)
		{
			var data = await _wikiTasks.GetPageDiff(path.Trim('/'), fromRevision, toRevision);
			return Json(data);
		}
	}
}
