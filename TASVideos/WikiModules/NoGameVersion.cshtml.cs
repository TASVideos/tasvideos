using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.NoGameVersion)]
public class NoGameVersion(ApplicationDbContext db) : WikiViewComponent
{
	public List<NoHashEntry> GameVersions { get; set; } = [];
	public List<NoGame.Entry> Submissions { get; set; } = [];

	public async Task<IViewComponentResult> InvokeAsync()
	{
		GameVersions = await db.GameVersions
			.Where(v => v.Sha1 == null || v.Md5 == null)
			.Select(v => new NoHashEntry(
				v.Id,
				v.Name,
				v.GameId,
				v.Game!.DisplayName,
				v.Sha1,
				v.Md5,
				v.System!.Code,
				v.Publications.Select(p => new NoGame.Entry(p.Id, p.Title)).ToList(),
				v.Submissions.Where(s => s.Status != SubmissionStatus.Published).Select(s => new NoGame.Entry(s.Id, s.Title)).ToList()))
			.ToListAsync();
		Submissions = await db.Submissions
			.Where(s => s.GameVersionId == null)
			.ThatAreInActive()
			.OrderBy(p => p.Id)
			.Select(s => new NoGame.Entry(s.Id, s.Title))
			.ToListAsync();

		return View();
	}

	public record NoHashEntry(
		int GameVersionId,
		string GameVersionName,
		int GameId,
		string GameName,
		string? Sha1,
		string? Md5,
		string SystemCode,
		List<NoGame.Entry> Pubs,
		List<NoGame.Entry> Subs);
}
