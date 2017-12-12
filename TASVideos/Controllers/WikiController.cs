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
			// TODO: can't edit a page without the page, check path is empty and if so...do something
			// TODO: grab page from db based on path
			var existingPage = await _wikiTasks.GetPage(path);

			WikiEditModel model;
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
				return Redirect("/" + model.PageName);
			}

			return View(model);
		}

		[HttpPost]
		public ContentResult GeneratePreview()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			var w = new StringWriter();
			Util.DebugWriteHtml(input, w);
			return Content(w.ToString(), "text/plain");
		}

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
				return View(Razor.WikiMarkupFileProvider.Prefix + existingPage.DbId);
			}

			return RedirectToAction("PageNotFound", new { url });
		}

		public async Task<IActionResult> ViewSource(string path)
		{
			var existingPage = await _wikiTasks.GetPage(path);

			var model = new WikiViewModel
			{
				PageName = path,
				Markup = existingPage?.Markup ?? "This page does not yet exist",
				DbId = existingPage?.DbId ?? 0
			};

			return View(model);
		}
    }
}
