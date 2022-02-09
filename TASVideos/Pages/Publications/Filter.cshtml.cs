using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class FilterModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IMovieSearchTokens _movieTokens;
	private readonly ITagService _tagService;
	private readonly IFlagService _flagService;

	public FilterModel(
		ApplicationDbContext db,
		IMovieSearchTokens movieTokens,
		ITagService tagService,
		IFlagService flagService)
	{
		_db = db;
		_movieTokens = movieTokens;
		_tagService = tagService;
		_flagService = flagService;
	}

	[BindProperty]
	public PublicationSearchModel Search { get; set; } = new()
	{
		Years = Enumerable.Empty<int>()
	};

	public IPublicationTokens Tokens { get; set; } = null!;

	public IEnumerable<SelectListItem> AvailableTags { get; set; } = new List<SelectListItem>();

	public IEnumerable<SelectListItem> AvailableFlags { get; set; } = new List<SelectListItem>();

	public IEnumerable<SelectListItem> AvailableGameGroups { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		Tokens = await _movieTokens.GetTokens();
		AvailableTags = (await _tagService.GetAll())
			.Select(t => new SelectListItem
			{
				Value = t.Code.ToLower(),
				Text = t.DisplayName
			})
			.OrderBy(t => t.Text);
		AvailableFlags = (await _flagService.GetAll())
			.Select(f => new SelectListItem
			{
				Value = f.Token.ToLower(),
				Text = f.Name
			})
			.OrderBy(t => t.Text);
		AvailableGameGroups = await _db.GameGroups
			.Select(gg => new SelectListItem
			{
				Value = gg.Id.ToString(),
				Text = gg.Name
			})
			.OrderBy(gg => gg.Text)
			.ToListAsync();
		return Page();
	}

	public IActionResult OnPost()
	{
		var page = $"/Movies-{Search.ToUrl()}";
		return BaseRedirect(page);
	}
}
