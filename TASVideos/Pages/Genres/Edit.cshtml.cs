using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Genres;

[RequirePermission(PermissionTo.TagMaintenance)]
public class EditModel : BasePageModel
{
	private readonly IGenreService _genreService;

	public EditModel(IGenreService genreService)
	{
		_genreService = genreService;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public GenreDto Genre { get; set; } = new(0, "", 0);

	public bool InUse { get; set; } = true;

	public async Task<IActionResult> OnGet()
	{
		var genre = await _genreService.GetById(Id);
		if (genre is null)
		{
			return NotFound();
		}

		Genre = genre;
		InUse = await _genreService.InUse(Id);
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var result = await _genreService.Edit(Id, Genre.DisplayName);
		switch (result)
		{
			default:
			case GenreChangeResult.Success:
				SuccessStatusMessage("Genre successfully updated.");
				return BasePageRedirect("Index");
			case GenreChangeResult.NotFound:
				return NotFound();
			case GenreChangeResult.Fail:
				ErrorStatusMessage($"Unable to edit Genre {Genre.DisplayName} due to an unknown error.");
				return Page();
		}
	}

	public async Task<IActionResult> OnPostDelete()
	{
		var result = await _genreService.Delete(Id);
		switch(result)
		{
			default:
			case GenreChangeResult.Success:
				SuccessStatusMessage($"Genre {Id}, deleted successfully.");
				break;
			case GenreChangeResult.InUse:
				ErrorStatusMessage($"Unable to delete Genre {Id}, the genre is in use by at least 1 game.");
				break;
			case GenreChangeResult.NotFound:
				ErrorStatusMessage($"Genre {Id}, not found.");
				break;
			case GenreChangeResult.Fail:
				ErrorStatusMessage($"Unable to delete Genre {Id}, the genre may have already been deleted or updated.");
				break;
		}

		return BasePageRedirect("Index");
	}
}
