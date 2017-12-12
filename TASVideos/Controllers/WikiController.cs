using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

		public async Task<IActionResult> Edit(string path)
		{
			path = path?.Trim('/');
			if (string.IsNullOrWhiteSpace(path))
			{
				return RedirectHome();
			}

			WikiEditModel model;
			var existingPage = await _wikiTasks.GetPage(path);

			if (existingPage != null)
			{
				model = new WikiEditModel
				{
					PageName = path,
					Markup = existingPage.Markup
				};
			}
			else
			{
				model = new WikiEditModel
				{
					PageName = path,
					Markup = ""
				};
			}

			return View(model);
		}

		// TODO: perms
		[HttpPost]
		public async Task<IActionResult> Edit(WikiEditModel model)
		{
			if (ModelState.IsValid)
			{
				await _wikiTasks.SavePage(model);
				return Redirect("/" + model.PageName.Trim('/'));
			}

			return View(model);
		}

		// TODO: perms
		[HttpPost]
		public ContentResult GeneratePreview()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			var w = new StringWriter();
			Util.DebugWriteHtml(input, w);
			return Content(w.ToString(), "text/plain");
		}

		[AllowAnonymous]
		public IActionResult PageNotFound(string url)
		{
			ViewData["Title"] = url?.Trim('/');
			return View();
		}

		[AllowAnonymous]
		public async Task<IActionResult> RenderWikiPage(string url)
		{
			var existingPage = await _wikiTasks.GetPage(url);

			if (existingPage != null)
			{
				return View(Razor.WikiMarkupFileProvider.Prefix + existingPage.Id, existingPage);
			}

			return RedirectToAction(nameof(PageNotFound), new { url });
		}

		// TODO: perms
		public async Task<IActionResult> ViewSource(string path, int? revision = null)
		{
			var existingPage = await _wikiTasks.GetPage(path, revision);

			if (existingPage != null)
			{
				return View(existingPage);
			}

			return RedirectHome();
		}
    }
}
