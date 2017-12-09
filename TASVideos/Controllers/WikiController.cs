using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public IActionResult Edit(string path)
		{
			// TODO: grab page from db based on path
			var model = new WikiEditModel
			{
				PageName = path ?? "/Dummy/Dummy",
				Markup = "Hello __World__"
			};

			return View(model);
		}

		// TODO: perms
		[HttpPost]
		public async Task<IActionResult> Edit(WikiEditModel model)
		{
			model.PageName = model.PageName.Trim('/');
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

		public IActionResult RenderWikiPage(string url)
		{
			if (url == "Boo/Far")
			{
				return Content("Boo Far", "text/plain");
			}
			else
			{
				return NotFound();
			}
		}
    }
}
