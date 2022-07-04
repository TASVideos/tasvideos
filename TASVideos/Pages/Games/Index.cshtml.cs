using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Games.Models;
using TASVideos.ViewComponents;

namespace TASVideos.Pages.Games;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	public GameDisplayModel Game { get; set; } = new();

	public IEnumerable<MiniMovieModel> Movies { get; set; } = new List<MiniMovieModel>();

	public async Task<IActionResult> OnGet()
	{
		var game = await _db.Games
			.Select(g => new GameDisplayModel
			{
				Id = g.Id,
				DisplayName = g.DisplayName,
				Abbreviation = g.Abbreviation,
				ScreenshotUrl = g.ScreenshotUrl,
				GameResourcesPage = g.GameResourcesPage,
				Genres = g.GameGenres.Select(gg => gg.Genre!.DisplayName),
				Versions = g.GameVersions.Select(gv => new GameDisplayModel.GameVersion
				{
					Type = gv.Type,
					Id = gv.Id,
					Md5 = gv.Md5,
					Sha1 = gv.Sha1,
					Name = gv.Name,
					Region = gv.Region,
					Version = gv.Version,
					SystemCode = gv.System!.Code,
					TitleOverride = gv.TitleOverride
				}).ToList(),
				GameGroups = g.GameGroups.Select(gg => new GameDisplayModel.GameGroup
				{
					Id = gg.GameGroupId,
					Name = gg.GameGroup!.Name
				}).ToList(),
				PublicationCount = g.Publications.Count(p => p.ObsoletedById == null),
				ObsoletePublicationCount = g.Publications.Count(p => p.ObsoletedById != null),
				SubmissionCount = g.Submissions.Count,
				UserFilesCount = g.UserFiles.Count(uf => !uf.Hidden)
			})
			.SingleOrDefaultAsync(g => g.Id == Id);

		if (game is null)
		{
			return NotFound();
		}

		Game = game;
		Movies = await _db.Publications
			.Where(p => p.GameId == Id && p.ObsoletedById == null)
			.OrderBy(p => p.Branch == null ? -1 : p.Branch.Length)
			.ThenBy(p => p.Frames)
			.ToMiniMovieModel()
			.ToListAsync();

		return Page();
	}
}
