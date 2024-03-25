using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class FilterModel(
	ApplicationDbContext db,
	IMovieSearchTokens movieTokens,
	ITagService tagService,
	IFlagService flagService)
	: BasePageModel
{
	[BindProperty]
	public PublicationSearchModel Search { get; set; } = new();

	[FromQuery]
	public string Query { get; set; } = "";

	public IPublicationTokens Tokens { get; set; } = null!;

	public List<SelectListItem> AvailableTags { get; set; } = [];

	public List<SelectListItem> AvailableFlags { get; set; } = [];

	public List<SelectListItem> AvailableGameGroups { get; set; } = [];

	public List<SelectListItem> AvailableAuthors { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		Tokens = await movieTokens.GetTokens();
		var tokensFromQuery = Query.ToTokens();
		Search = PublicationSearchModel.FromTokens(tokensFromQuery, Tokens);

		AvailableTags = [.. (await tagService.GetAll())
			.Select(t => new SelectListItem
			{
				Value = t.Code.ToLower(),
				Text = t.DisplayName
			})
			.OrderBy(t => t.Text)];
		AvailableFlags = [.. (await flagService.GetAll())
			.Select(f => new SelectListItem
			{
				Value = f.Token.ToLower(),
				Text = f.Name
			})
			.OrderBy(t => t.Text)];
		AvailableGameGroups = await db.GameGroups
			.Select(gg => new SelectListItem
			{
				Value = gg.Id.ToString(),
				Text = gg.Name
			})
			.OrderBy(gg => gg.Text)
			.ToListAsync();
		AvailableAuthors = await db.Users
			.ThatArePublishedAuthors()
			.Select(u => new SelectListItem
			{
				Value = u.Id.ToString(),
				Text = u.UserName
			})
			.ToListAsync();
		return Page();
	}

	public IActionResult OnPost()
	{
		var page = $"/Movies-{Search.ToUrl()}";
		return BaseRedirect(page);
	}
}
