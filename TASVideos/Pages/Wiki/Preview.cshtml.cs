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

			// ViewData["WikiPage"] = null;
			// ViewData["Title"] = "Generated Preview";
			// ViewData["Layout"] = null;

			var sw = new StringWriter(); // TODO: is there a better way to do this without StringWriter, like, by streaming to the page or something?
			Util.RenderHtml(input, sw);
			Html = sw.ToString();
			return Page();
		}
	}
}
