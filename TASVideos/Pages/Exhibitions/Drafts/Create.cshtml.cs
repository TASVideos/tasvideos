using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Exhibition;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;
using TASVideos.Pages.Exhibitions.Drafts.Models;
using TASVideos.Pages.Exhibitions.Models;

namespace TASVideos.Pages.Exhibitions.Drafts;

public class CreateModel : PageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IMediaFileUploader _uploader;
	private readonly IMovieParser _parser;
	public CreateModel(ApplicationDbContext db, IMediaFileUploader uploader, IMovieParser parser)
	{
		_db = db;
		_uploader = uploader;
		_parser = parser;
	}

	[BindProperty]
	public ExhibitionDraftCreateModel Exhibition { get; set; } = new();
	public List<SelectListItem> AvailableGames { get; set; } = [];
	public List<SelectListItem> AvailableUsers { get; set; } = [];

	public async Task OnGet()
	{
		await PopulateDropdowns();
	}

	public async Task<IActionResult> OnPost()
	{
		if (Exhibition.Screenshot != null && !Exhibition.Screenshot.IsValidImage())
		{
			ModelState.AddModelError($"{nameof(Exhibition)}.{nameof(Exhibition.Screenshot)}", "Invalid file type. Must be .png or .jpg");
		}

		if (!ModelState.IsValid)
		{
			await PopulateDropdowns();
			return Page();
		}

		if (Exhibition.MovieFile != null)
		{
			var parseResult = await _parser.ParseZip(Exhibition.MovieFile.OpenReadStream());

			if (!parseResult.Success)
			{
				ModelState.AddParseErrors(parseResult, $"{nameof(Exhibition)}.{nameof(Exhibition.MovieFile)}");
				await PopulateDropdowns();
				return Page();
			}
		}

		Exhibition exhibition = new()
		{
			Title = Exhibition.Title,
			ExhibitionTimestamp = Exhibition.ExhibitionTimestamp,
			Games = await _db.Games.Where(g => Exhibition.Games.Contains(g.Id)).ToListAsync(),
			Contributors = await _db.Users.Where(g => Exhibition.Contributors.Contains(g.Id)).ToListAsync()
		};

		if (Exhibition.MovieFile != null)
		{
			byte[] movieFile = await Exhibition.MovieFile.ToBytes();

			exhibition.Files.Add(new ExhibitionFile
			{
				Type = ExhibitionFileType.MovieFile,
				Description = Exhibition.MovieFileDescription,
				FileData = movieFile,
			});
		}

		await _db.Exhibitions.AddAsync(exhibition);

		await _db.SaveChangesAsync();

		if (Exhibition.Screenshot != null)
		{
			await _uploader.UploadExhibitionScreenshot(exhibition.Id, Exhibition.Screenshot, Exhibition.ScreenshotDescription);
		}

		return Page();
	}

	private async Task PopulateDropdowns()
	{
		AvailableGames = await _db.Games
			.OrderBy(g => g.DisplayName)
			.Select(g => new SelectListItem
			{
				Text = g.DisplayName,
				Value = g.Id.ToString()
			})
			.ToListAsync();

		AvailableUsers = await _db.Users
			.OrderBy(u => u.UserName)
			.Select(u => new SelectListItem
			{
				Text = u.UserName,
				Value = u.Id.ToString()
			})
			.ToListAsync();
	}
}
