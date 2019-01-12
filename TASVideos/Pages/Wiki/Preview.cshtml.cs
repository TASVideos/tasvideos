using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Razor;
using TASVideos.Tasks;
using TASVideos.WikiEngine;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	[IgnoreAntiforgeryToken]
	public class PreviewModel : BasePageModel
	{
		private readonly WikiMarkupFileProvider _wikiMarkupFileProvider;

		public PreviewModel(
			WikiMarkupFileProvider wikiMarkupFileProvider,
			UserTasks userTasks)
			: base(userTasks)
		{
			_wikiMarkupFileProvider = wikiMarkupFileProvider;
		}

		public string Html { get; set; } = "";

		public IActionResult OnPost()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			var sw = new StringWriter();
			Util.RenderHtml(input, sw);
			return Content(sw.ToString(), "text/plain"); // really HTML, but a fragment so `text/plain` is good enough
		}
	}
}
