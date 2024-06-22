namespace TASVideos.Pages.Games;

[AllowAnonymous]
public class PublicationHistoryModel(IPublicationHistory history) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public PublicationHistoryGroup History { get; set; } = new();

	[FromQuery]
	public int? Highlight { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var gameHistory = await history.ForGame(Id);
		if (gameHistory is null)
		{
			return NotFound();
		}

		History = gameHistory;
		return Page();
	}
}
