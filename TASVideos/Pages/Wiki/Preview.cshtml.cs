using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Razor;
using TASVideos.Tasks;

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

		public IActionResult OnPost()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();

			ViewData["WikiPage"] = null;
			ViewData["Title"] = "Generated Preview";
			ViewData["Layout"] = null;
			var name = _wikiMarkupFileProvider.SetPreviewMarkup(input);

			return Partial(name);
		}
	}
}
