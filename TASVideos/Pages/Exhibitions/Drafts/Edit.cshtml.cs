using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity.Exhibition;
using TASVideos.MovieParsers;
using TASVideos.Pages.Exhibitions.Drafts.Models;
using TASVideos.Pages.Exhibitions.Models;

namespace TASVideos.Pages.Exhibitions.Drafts;

public class EditModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IMediaFileUploader _uploader;
	private readonly IMovieParser _parser;
	private readonly IWikiPages _wikiPages;
	private readonly ITASVideoAgent _tasVideoAgent;
	public EditModel(
		ApplicationDbContext db,
		IMediaFileUploader uploader,
		IMovieParser parser,
		IWikiPages wikiPages,
		ITASVideoAgent tasVideoAgent)
	{
		_db = db;
		_uploader = uploader;
		_parser = parser;
		_wikiPages = wikiPages;
		_tasVideoAgent = tasVideoAgent;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public ExhibitionFormModel ExhibitionForm { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var exhibition = await _db.Exhibitions
			.Where(e => e.Id == Id)
			.Select(e => new ExhibitionAddEditModel
			{
				Title = e.Title,
				ExhibitionTimestamp = e.ExhibitionTimestamp,
				Games = e.Games.Select(g => g.Id).ToList(),
				Contributors = e.Contributors.Select(e => e.Id).ToList(),
				Movies = e.Files.Where(f => f.Type == ExhibitionFileType.MovieFile).Select(m => new ExhibitionAddEditModel.ExhibitionAddEditMovieModel
				{
					FileId = m.Id,
					MovieFileDescription = m.Description,
				}).ToList(),
				Urls = e.Urls.Select(u => new ExhibitionAddEditModel.ExhibitionAddEditUrlModel
				{
					UrlId = u.Id,
					Type = u.Type,
					DisplayName = u.DisplayName ?? "",
					Url = u.Url,
				}).ToList(),
			})
			.SingleOrDefaultAsync();

		if (exhibition is null)
		{
			return NotFound();
		}

		var page = await _wikiPages.Page(WikiHelper.ToExhibitionWikiPageName(Id));

		exhibition.Markup = page?.Markup ?? "";

		ExhibitionForm.Type = ExhibitionFormModel.FormType.Edit;
		ExhibitionForm.Exhibition = exhibition;

		await PopulateDropdowns();

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var test = 2;
		return Page();
	}

	private async Task PopulateDropdowns()
	{
		ExhibitionForm.AvailableGames = await _db.Games
			.OrderBy(g => g.DisplayName)
			.Select(g => new SelectListItem
			{
				Text = g.DisplayName,
				Value = g.Id.ToString()
			})
			.ToListAsync();

		ExhibitionForm.AvailableUsers = await _db.Users
			.OrderBy(u => u.UserName)
			.Select(u => new SelectListItem
			{
				Text = u.UserName,
				Value = u.Id.ToString()
			})
			.ToListAsync();
	}
}
