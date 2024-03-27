using System.Text;
using TASVideos.Core.Services.Wiki;

namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class PreviewModel(IWikiPages pages) : BasePageModel
{
	public string Markup { get; set; } = "";

	[FromQuery]
	public string? Path { get; set; }

	public IWikiPage PageData { get; set; } = null!;

	public async Task<IActionResult> OnPost()
	{
		Markup = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();
		if (Path is not null)
		{
			var pageData = await pages.Page(Path);
			if (pageData is null)
			{
				return NotFound();
			}

			PageData = pageData;
		}

		return Page();
	}
}
