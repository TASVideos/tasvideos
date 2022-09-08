using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Systems;

[RequirePermission(PermissionTo.GameSystemMaintenance)]
public class EditFramerateModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IGameSystemService _gameSystemService;

	public EditFramerateModel(ApplicationDbContext db, IGameSystemService gameSystemService)
	{
		_db = db;
		_gameSystemService = gameSystemService;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public FrameRateEditModel FrameRate { get; set; } = new();

	public IReadOnlyCollection<UsageEntry> PublicationEntries = new List<UsageEntry>();
	public IReadOnlyCollection<UsageEntry> SubmissionEntries = new List<UsageEntry>();

	public bool InUse => PublicationEntries.Any() || SubmissionEntries.Any();

	public async Task<IActionResult> OnGet()
	{
		var frameRate = await _db.GameSystemFrameRates
			.Where(sf => sf.Id == Id)
			.Select(sf => new FrameRateEditModel
			{
				SystemId = sf.System!.Id,
				SystemCode = sf.System!.Code,
				FrameRate = sf.FrameRate,
				RegionCode = sf.RegionCode,
				Preliminary = sf.Preliminary,
				Obsolete = sf.Obsolete
			})
			.SingleOrDefaultAsync();

		if (frameRate is null)
		{
			return NotFound();
		}

		FrameRate = frameRate;

		await PopulateUsages();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var frameRate = await _db.GameSystemFrameRates
			.Include(sf => sf.System)
			.SingleOrDefaultAsync(sf => sf.Id == Id);

		if (frameRate is null)
		{
			return NotFound();
		}

		frameRate.FrameRate = FrameRate.FrameRate;
		frameRate.RegionCode = FrameRate.RegionCode;
		frameRate.Preliminary = FrameRate.Preliminary;
		frameRate.Obsolete = FrameRate.Obsolete;

		var displayName = $"{FrameRate.SystemCode} {FrameRate.RegionCode} {FrameRate.FrameRate}";
		await ConcurrentSave(
			_db,
			$"FrameRate {displayName} updated.",
			$"Unable to updated {displayName} due to an unknown error");
		await _gameSystemService.FlushCache();
		return BasePageRedirect("Edit", new { Id = FrameRate.SystemId });
	}

	public async Task<IActionResult> OnPostDelete(int systemId)
	{
		var frameRate = await _db.GameSystemFrameRates
			.Include(sf => sf.System)
			.SingleOrDefaultAsync(sf => sf.Id == Id);

		if (frameRate is null)
		{
			return NotFound();
		}

		await PopulateUsages();
		if (InUse)
		{
			return BadRequest("Unable to delete a Framerate that is in use.");
		}

		_db.GameSystemFrameRates.Remove(frameRate);
		await ConcurrentSave(_db, $"FrameRate {Id} deleted", $"Unable to delete FrameRate {Id}");

		return BasePageRedirect("Edit", new { Id = systemId });
	}

	private async Task PopulateUsages()
	{
		PublicationEntries = await _db.Publications
			.Where(p => p.SystemFrameRateId == Id)
			.Select(p => new UsageEntry(p.Id, p.Title))
			.ToListAsync();

		SubmissionEntries = await _db.Submissions
			.Where(s => s.SystemFrameRateId == Id)
			.Select(s => new UsageEntry(s.Id, s.Title))
			.ToListAsync();
	}

	// TODO: move me
	public class FrameRateEditModel
	{
		public int SystemId { get; init; }
		public string SystemCode { get; init; } = "";
		public double FrameRate { get; init; }

		[Required]
		[StringLength(8)]
		public string RegionCode { get; init; } = "";
		public bool Preliminary { get; init; }

		public bool Obsolete { get; init; }
	}

	public record UsageEntry(int Id, string Title);
}
