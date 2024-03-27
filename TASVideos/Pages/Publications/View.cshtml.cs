using System.Net.Mime;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class ViewModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public PublicationDisplayModel Publication { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var publication = await db.Publications
			.ToViewModel(false, User.GetUserId())
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (publication is null)
		{
			return NotFound();
		}

		Publication = publication;
		ViewData["ReturnUrl"] = HttpContext.CurrentPathToReturnUrl();
		return Page();
	}

	public async Task<IActionResult> OnGetDownload()
	{
		var pub = await db.Publications
			.Where(s => s.Id == Id)
			.Select(s => new { s.MovieFile, s.MovieFileName })
			.SingleOrDefaultAsync();

		if (pub is null)
		{
			return NotFound();
		}

		return File(pub.MovieFile, MediaTypeNames.Application.Octet, $"{pub.MovieFileName}.zip");
	}

	public async Task<IActionResult> OnGetDownloadAdditional(int fileId)
	{
		var file = await db.PublicationFiles
			.Where(pf => pf.PublicationId == Id)
			.Where(pf => pf.Id == fileId)
			.Select(pf => new { pf.FileData, pf.Path })
			.SingleOrDefaultAsync();

		if (file?.FileData is null)
		{
			return NotFound();
		}

		return File(file.FileData, MediaTypeNames.Application.Octet, $"{file.Path}.zip");
	}
}
