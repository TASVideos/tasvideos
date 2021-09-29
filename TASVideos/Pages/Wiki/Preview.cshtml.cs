using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.Youtube;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	[IgnoreAntiforgeryToken]
	public class PreviewModel : BasePageModel
	{
		private readonly IWikiPages _pages;
		private readonly IWikiToTextRenderer _renderer;

		public PreviewModel(IWikiPages pages, IWikiToTextRenderer renderer)
		{
			_pages = pages;
			_renderer = renderer;
		}

		public string Markup { get; set; } = "";

		[FromQuery]
		public int? Id { get; set; }

		public WikiPage PageData { get; set; } = new ();

		public async Task<IActionResult> OnPost()
		{
			Markup = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();

			var str = await _renderer.RenderWikiForYoutube(new WikiPage { Markup = Markup });
			return new ContentResult { Content = str };

			////if (Id.HasValue)
			////{
			////	PageData = await _pages.Revision(Id.Value);
			////}

			////return Page();
		}
	}
}
