using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class ViewModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public ViewModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	public PublicationDisplayModel Publication { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var publication = await _db.Publications
			.ToViewModel(false, User.GetUserId())
			.SingleOrDefaultAsync(p => p.Id == Id);

		if (publication == null)
		{
			return NotFound();
		}

		Publication = publication;
		return Page();
	}

	public async Task<IActionResult> OnGetDownload()
	{
		var pub = await _db.Publications
			.Where(s => s.Id == Id)
			.Select(s => new { s.MovieFile, s.MovieFileName })
			.SingleOrDefaultAsync();

		if (pub == null)
		{
			return NotFound();
		}

		return File(pub.MovieFile, MediaTypeNames.Application.Octet, $"{pub.MovieFileName}.zip");
	}

	public async Task<IActionResult> OnGetDownloadAdditional(int fileId)
	{
		var file = await _db.PublicationFiles
			.Where(pf => pf.Id == fileId)
			.Select(pf => new { pf.FileData, pf.Path })
			.SingleOrDefaultAsync();

		if (file?.FileData == null)
		{
			return NotFound();
		}

		return File(file.FileData, MediaTypeNames.Application.Octet, $"{file.Path}.zip");
	}
}
