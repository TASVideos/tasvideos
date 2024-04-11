using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.Wiki;
using TASVideos.MovieParsers;
using TASVideos.Pages.Exhibitions.Drafts.Models;

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
	public ExhibitionDraftEditModel Exhibition { get; set; } = new();
	public List<SelectListItem> AvailableGames { get; set; } = [];
	public List<SelectListItem> AvailableUsers { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		var exhibition = await _db.Exhibitions
			.Where(e => e.Id == Id)
			.Select(e => new ExhibitionDraftEditModel
			{
				Title = e.Title,
				ExhibitionTimestamp = e.ExhibitionTimestamp,
				Games = e.Games.Select(g => g.Id).ToList(),
				Contributors = e.Contributors.Select(e => e.Id).ToList(),
				Urls = e.Urls.Select(u => new ExhibitionDraftEditModel.ExhibitionDraftEditUrlModel
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

		Exhibition = exhibition;

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
