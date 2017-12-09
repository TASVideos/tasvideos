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
		public WikiController(UserTasks userTasks)
			: base(userTasks)
		{
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

		[HttpPost]
		public IActionResult Edit(WikiEditModel model)
		{
			if (ModelState.IsValid)
			{
				// TODO
				RedirectHome();
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
    }
}
