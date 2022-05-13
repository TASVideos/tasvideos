using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Games;

[RequirePermission(PermissionTo.RewireGames)]
public class RewireModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public RewireModel(
		ApplicationDbContext db)
	{
		_db = db;
	}

	[FromQuery]
	[Display(Name = "From Game Id")]
	public int? FromGameId { get; set; }

	[FromQuery]
	[Display(Name = "Into Game Id")]
	public int? IntoGameId { get; set; }

	public bool ValidIds { get; set; } = false;

	public RewireEntry? FromGame { get; set; }
	public RewireEntry? IntoGame { get; set; }

	public class RewireEntry
	{
		public Entry? Game;
		public ICollection<EntryWithRom>? Publications;
		public ICollection<EntryWithRom>? Submissions;
		public ICollection<Entry>? Roms;
		public ICollection<EntryLong>? Userfiles;
		public ICollection<Entry>? RamAddresses;
	}

	public record Entry(int Id, string Title);
	public record EntryWithRom(int Id, string Title, string? RomName);
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
					Publications = g.Publications.Select(p => new EntryWithRom(p.Id, p.Title, p.Rom == null ? null : p.Rom.TitleOverride)).ToList(),
					Submissions = g.Submissions.Select(s => new EntryWithRom(s.Id, s.Title, s.Rom == null ? null : s.Rom.TitleOverride)).ToList(),
					Roms = g.Roms.Select(r => new Entry(r.Id, r.Name)).ToList(),
					Userfiles = g.UserFiles.Select(u => new EntryLong(u.Id, u.Title)).ToList(),
				})
				.FirstOrDefaultAsync();
			FromGame!.RamAddresses = await _db.GameRamAddresses
				.Where(a => a.GameId == FromGameId)
				.Select(a => new Entry(a.Id, a.Address.ToString()))
				.ToListAsync();

			IntoGame = await _db.Games
				.Where(g => g.Id == IntoGameId)
				.Select(g => new RewireEntry
				{
					Game = new Entry(g.Id, g.DisplayName),
					Publications = g.Publications.Select(p => new EntryWithRom(p.Id, p.Title, p.Rom == null ? null : p.Rom.TitleOverride)).ToList(),
					Submissions = g.Submissions.Select(s => new EntryWithRom(s.Id, s.Title, s.Rom == null ? null : s.Rom.TitleOverride)).ToList(),
					Roms = g.Roms.Select(r => new Entry(r.Id, r.Name)).ToList(),
					Userfiles = g.UserFiles.Select(u => new EntryLong(u.Id, u.Title)).ToList(),
				})
				.FirstOrDefaultAsync();
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
				var rewireGames = await _db.Games
					.Include(g => g.Publications)
					.Include(g => g.Submissions)
					.Include(g => g.Roms)
					.Include(g => g.UserFiles)
					.Where(g => g.Id == FromGameId)
					.FirstOrDefaultAsync();
				foreach (var pub in rewireGames!.Publications)
				{
					pub.GameId = intoGameId;
				}

				foreach (var sub in rewireGames!.Submissions)
				{
					sub.GameId = intoGameId;
				}

				foreach (var rom in rewireGames!.Roms)
				{
					rom.GameId = intoGameId;
				}

				foreach (var userfile in rewireGames!.UserFiles)
				{
					userfile.GameId = intoGameId;
				}

				var rewireRamAddresses = await _db.GameRamAddresses
				.Where(a => a.GameId == FromGameId)
				.ToListAsync();

				foreach (var address in rewireRamAddresses)
				{
					address.GameId = intoGameId;
				}

				await ConcurrentSave(_db, $"Rewired Game {FromGameId} into Game {IntoGameId}", $"Unable to rewire Game {FromGameId} into Game {IntoGameId}");
			}
		}

		return RedirectToPage("Rewire", new { FromGameId, IntoGameId });
	}
}
