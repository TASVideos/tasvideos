using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class IndexModel(
	ApplicationDbContext db,
	IMovieSearchTokens movieTokens) : BasePageModel
{
	[FromQuery]
	public PublicationRequest Paging { get; set; } = new();

	[FromRoute]
	public string Query { get; set; } = "";

	public PageOf<PublicationDisplayModel> Movies { get; set; } = PageOf<PublicationDisplayModel>.Empty();

	public async Task<IActionResult> OnGet()
	{
		var tokenLookup = await movieTokens.GetTokens();
		var tokens = Query.ToTokens();
		var searchModel = PublicationSearchModel.FromTokens(tokens, tokenLookup);

		// If no valid filter criteria, don't attempt to generate a list (else it would be all movies for what is most likely a malformed URL)
		if (searchModel.IsEmpty)
		{
			return BaseRedirect("Movies");
		}

		Movies = await db.Publications
			.FilterByTokens(searchModel)
			.ToViewModel(searchModel.SortBy == "y", User.GetUserId())
			.PageOf(Paging);
		ViewData["ReturnUrl"] = HttpContext.CurrentPathToReturnUrl();
		return Page();
	}
}
