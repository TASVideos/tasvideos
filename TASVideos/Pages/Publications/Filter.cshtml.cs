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
	public IndexModel.PublicationSearch Search { get; set; } = new();

	[FromQuery]
	public string Query { get; set; } = "";

	public IPublicationTokens Tokens { get; set; } = null!;

	public List<SelectListItem> AvailableTags { get; set; } = [];

	public List<SelectListItem> AvailableFlags { get; set; } = [];

	public List<SelectListItem> AvailableGameGroups { get; set; } = [];

	public List<SelectListItem> AvailableAuthors { get; set; } = [];

	public async Task OnGet()
	{
		Tokens = await movieTokens.GetTokens();
		var tokensFromQuery = Query.ToTokens();
		Search = IndexModel.PublicationSearch.FromTokens(tokensFromQuery, Tokens);

		AvailableTags = [.. (await tagService.GetAll()).ToDropDown()];
		AvailableFlags = [.. (await flagService.GetAll()).ToDropDown()];
		AvailableGameGroups = await db.GameGroups.ToDropDownList();
		AvailableAuthors = await db.Users
			.ThatArePublishedAuthors()
			.ToDropdownList();
	}

	public IActionResult OnPost()
	{
		var page = $"/Movies-{Search.ToUrl()}";
		return BaseRedirect(page);
	}
}
