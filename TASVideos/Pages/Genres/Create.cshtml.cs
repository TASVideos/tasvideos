using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Genres;

[RequirePermission(PermissionTo.TagMaintenance)]
public class CreateModel : BasePageModel
{
	private readonly IGenreService _genreService;

	public CreateModel(IGenreService genreService)
	{
		_genreService = genreService;
	}

	[BindProperty]
	public Genre Genre { get; set; } = new();

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var id = await _genreService.Add(Genre.DisplayName);

		if (id.HasValue)
		{
			SuccessStatusMessage($"Genre {Genre.DisplayName} sucessfully created.");
			return BasePageRedirect("Index");
		}

		ErrorStatusMessage("Unable to create genre due to an unknown error.");
		return Page();
	}
}
