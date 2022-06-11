using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Games;

[RequirePermission(PermissionTo.RewireGames)]
public class RewireModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;

	public RewireModel(
		ApplicationDbContext db,
		ExternalMediaPublisher publisher)
	{
		_db = db;
		_publisher = publisher;
	}

	[FromQuery]
	[Display(Name = "From Game Id")]
	public int? FromGameId { get; set; }

	[FromQuery]
	[Display(Name = "Into Game Id")]
	public int? IntoGameId { get; set; }

	public bool ValidIds { get; set; }

	public RewireEntry? FromGame { get; set; }
	public RewireEntry? IntoGame { get; set; }

	public class RewireEntry
	{
		public Entry? Game;
		public ICollection<EntryWithVersion>? Publications;
		public ICollection<EntryWithVersion>? Submissions;
		public ICollection<Entry>? Versions;
		public ICollection<EntryLong>? Userfiles;
		public ICollection<Entry>? RamAddresses;
	}

	public record Entry(int Id, string Title);
	public record EntryWithVersion(int Id, string Title, string? VersionName);
	public record EntryLong(long Id, string Title);

	public async Task OnGet()
	{
		ValidIds = await _db.Games
			.Where(g => g.Id == FromGameId || g.Id == IntoGameId)
			.CountAsync() == 2;
		if (ValidIds)
		{
			FromGame = await _db.Games
				.Where(g => g.Id == FromGameId)
				.Select(g => new RewireEntry
				{
					Game = new Entry(g.Id, g.DisplayName),
					Publications = g.Publications.Select(p => new EntryWithVersion(p.Id, p.Title, p.GameVersion == null ? null : p.GameVersion.TitleOverride)).ToList(),
					Submissions = g.Submissions.Select(s => new EntryWithVersion(s.Id, s.Title, s.GameVersion == null ? null : s.GameVersion.TitleOverride)).ToList(),
					Versions = g.GameVersions.Select(r => new Entry(r.Id, r.Name)).ToList(),
					Userfiles = g.UserFiles.Select(u => new EntryLong(u.Id, u.Title)).ToList(),
				})
				.SingleAsync();
			FromGame!.RamAddresses = await _db.GameRamAddresses
				.Where(a => a.GameId == FromGameId)
				.Select(a => new Entry(a.Id, a.Address.ToString()))
				.ToListAsync();

			IntoGame = await _db.Games
				.Where(g => g.Id == IntoGameId)
				.Select(g => new RewireEntry
				{
					Game = new Entry(g.Id, g.DisplayName),
					Publications = g.Publications.Select(p => new EntryWithVersion(p.Id, p.Title, p.GameVersion == null ? null : p.GameVersion.TitleOverride)).ToList(),
					Submissions = g.Submissions.Select(s => new EntryWithVersion(s.Id, s.Title, s.GameVersion == null ? null : s.GameVersion.TitleOverride)).ToList(),
					Versions = g.GameVersions.Select(r => new Entry(r.Id, r.Name)).ToList(),
					Userfiles = g.UserFiles.Select(u => new EntryLong(u.Id, u.Title)).ToList(),
				})
				.SingleAsync();
			IntoGame!.RamAddresses = await _db.GameRamAddresses
				.Where(a => a.GameId == IntoGameId)
				.Select(a => new Entry(a.Id, a.Address.ToString()))
				.ToListAsync();
		}
	}

	public async Task<IActionResult> OnPost()
	{
		if (FromGameId is not null && IntoGameId is not null)
		{
			ValidIds = await _db.Games
				.Where(g => g.Id == FromGameId || g.Id == IntoGameId)
				.CountAsync() == 2;
			if (ValidIds)
			{
				int intoGameId = (int)IntoGameId;

				var rewirePublications = await _db.Publications
					.Where(p => p.GameId == FromGameId)
					.Select(p => new Publication { Id = p.Id })
					.ToListAsync();
				_db.Publications.AttachRange(rewirePublications);
				rewirePublications.ForEach(p => p.GameId = intoGameId);

				var rewireSubmissions = await _db.Submissions
					.Where(s => s.GameId == FromGameId)
					.Select(s => new Submission { Id = s.Id })
					.ToListAsync();
				_db.Submissions.AttachRange(rewireSubmissions);
				rewireSubmissions.ForEach(s => s.GameId = intoGameId);

				var rewireVersions = await _db.GameVersions
					.Where(r => r.GameId == FromGameId)
					.Select(r => new GameVersion { Id = r.Id })
					.ToListAsync();
				_db.GameVersions.AttachRange(rewireVersions);
				rewireVersions.ForEach(r => r.GameId = intoGameId);

				var rewireUserfiles = await _db.UserFiles
					.Where(u => u.GameId == FromGameId)
					.Select(u => new UserFile { Id = u.Id })
					.ToListAsync();
				_db.UserFiles.AttachRange(rewireUserfiles);
				rewireUserfiles.ForEach(u => u.GameId = intoGameId);

				var rewireRamAddresses = await _db.GameRamAddresses
					.Where(a => a.GameId == FromGameId)
					.Select(a => new GameRamAddress { Id = a.Id })
					.ToListAsync();
				_db.GameRamAddresses.AttachRange(rewireRamAddresses);
				rewireRamAddresses.ForEach(a => a.GameId = intoGameId);

				var result = await ConcurrentSave(_db, $"Rewired Game {FromGameId} into Game {IntoGameId}", $"Unable to rewire Game {FromGameId} into Game {IntoGameId}");
				if (result)
				{
					await _publisher.SendGameManagement($"{IntoGameId}G edited by {User.Name()}", $"Rewired {FromGameId}G into {IntoGameId}G", $"{IntoGameId}G");
				}
			}
		}

		return RedirectToPage("Rewire", new { FromGameId, IntoGameId });
	}
}
