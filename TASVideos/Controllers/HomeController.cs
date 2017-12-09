using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class HomeController : BaseController
	{
		public HomeController(UserTasks userTasks)
			: base(userTasks)
		{
		}

		public IActionResult Index()
		{
			return View(UserPermissions);
		}

		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult WikiTest()
		{
			return View();
		}

		public IActionResult WikiTestAjax()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			var output = TASVideos.WikiEngine.Util.DebugParseWikiPage(input);
			var w = new StringWriter();
			TASVideos.WikiEngine.Util.DebugWriteHtml(input, w);
			return Content(output + "\n" + w.ToString(), "text/plain");
		}
	}
}
