using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games;

[RequirePermission(PermissionTo.RewireGames)]
public class RewireModel(ApplicationDbContext db, IExternalMediaPublisher publisher) : BasePageModel
{
	[FromQuery]
	public int? FromGameId { get; set; }

	[FromQuery]
	public int? IntoGameId { get; set; }

	public bool ValidIds { get; set; }

	public RewireEntry? FromGame { get; set; }
	public RewireEntry? IntoGame { get; set; }

	public async Task OnGet()
	{
		ValidIds = await db.Games.CountAsync(g => g.Id == FromGameId || g.Id == IntoGameId) == 2;
		if (ValidIds)
		{
			FromGame = await db.Games
				.Where(g => g.Id == FromGameId)
				.Select(g => new RewireEntry
				{
					Game = new Entry(g.Id, g.DisplayName),
					Publications = g.Publications.Select(p => new EntryWithVersion(p.Id, p.Title, p.GameVersion == null ? null : p.GameVersion.TitleOverride)).ToList(),
					Submissions = g.Submissions.Select(s => new EntryWithVersion(s.Id, s.Title, s.GameVersion == null ? null : s.GameVersion.TitleOverride)).ToList(),
					Versions = g.GameVersions.Select(r => new Entry(r.Id, r.Name)).ToList(),
					Userfiles = g.UserFiles.Select(u => new EntryLong(u.Id, u.Title)).ToList()
				})
				.SingleAsync();

			IntoGame = await db.Games
				.Where(g => g.Id == IntoGameId)
				.Select(g => new RewireEntry
				{
					Game = new Entry(g.Id, g.DisplayName),
					Publications = g.Publications.Select(p => new EntryWithVersion(p.Id, p.Title, p.GameVersion == null ? null : p.GameVersion.TitleOverride)).ToList(),
					Submissions = g.Submissions.Select(s => new EntryWithVersion(s.Id, s.Title, s.GameVersion == null ? null : s.GameVersion.TitleOverride)).ToList(),
					Versions = g.GameVersions.Select(r => new Entry(r.Id, r.Name)).ToList(),
					Userfiles = g.UserFiles.Select(u => new EntryLong(u.Id, u.Title)).ToList()
				})
				.SingleAsync();
		}
	}

	public async Task<IActionResult> OnPost()
	{
		if (FromGameId is not null && IntoGameId is not null)
		{
			ValidIds = await db.Games.CountAsync(g => g.Id == FromGameId || g.Id == IntoGameId) == 2;
			if (ValidIds)
			{
				int intoGameId = (int)IntoGameId;

				var rewirePublications = await db.Publications
					.Where(p => p.GameId == FromGameId)
					.Select(p => new Publication { Id = p.Id })
					.ToListAsync();
				db.Publications.AttachRange(rewirePublications);
				rewirePublications.ForEach(p => p.GameId = intoGameId);

				var rewireSubmissions = await db.Submissions
					.Where(s => s.GameId == FromGameId)
					.Select(s => new Submission { Id = s.Id })
					.ToListAsync();
				db.Submissions.AttachRange(rewireSubmissions);
				rewireSubmissions.ForEach(s => s.GameId = intoGameId);

				var rewireVersions = await db.GameVersions
					.Where(r => r.GameId == FromGameId)
					.Select(r => new GameVersion { Id = r.Id })
					.ToListAsync();
				db.GameVersions.AttachRange(rewireVersions);
				rewireVersions.ForEach(r => r.GameId = intoGameId);

				var rewireUserfiles = await db.UserFiles
					.Where(u => u.GameId == FromGameId)
					.Select(u => new UserFile { Id = u.Id })
					.ToListAsync();
				db.UserFiles.AttachRange(rewireUserfiles);
				rewireUserfiles.ForEach(u => u.GameId = intoGameId);
				var result = await db.TrySaveChanges();
				SetMessage(result, $"Rewired Game {FromGameId} into Game {IntoGameId}", $"Unable to rewire Game {FromGameId} into Game {IntoGameId}");
				if (result.IsSuccess())
				{
					await publisher.SendGameManagement(
						$"[{IntoGameId}G]({{0}}) edited by {User.Name()}",
						$"Rewired {FromGameId}G into {IntoGameId}G",
						$"{IntoGameId}G");
				}
			}
		}

		return RedirectToPage("Rewire", new { FromGameId, IntoGameId });
	}

	public class RewireEntry
	{
		public Entry? Game { get; init; }
		public ICollection<EntryWithVersion>? Publications { get; init; }
		public ICollection<EntryWithVersion>? Submissions { get; init; }
		public ICollection<Entry>? Versions { get; init; }
		public ICollection<EntryLong>? Userfiles { get; init; }
	}

	public record Entry(int Id, string Title);
	public record EntryWithVersion(int Id, string Title, string? VersionName);
	public record EntryLong(long Id, string Title);
}
