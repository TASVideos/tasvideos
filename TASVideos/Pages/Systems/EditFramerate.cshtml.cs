using System.Globalization;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Pages.Systems;

[RequirePermission(PermissionTo.GameSystemMaintenance)]
public class EditFramerateModel(ApplicationDbContext db, IGameSystemService gameSystemService) : BasePageModel
{
	protected static readonly List<SelectListItem> RegionTypeList = Enum
		.GetValues<RegionType>()
		.ToDropDown();

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public FrameRateEdit FrameRate { get; set; } = new();

	public List<UsageEntry> PublicationEntries = [];
	public List<UsageEntry> SubmissionEntries = [];

	public bool InUse => PublicationEntries.Any() || SubmissionEntries.Any();

	public List<SelectListItem> RegionTypes => RegionTypeList;

	public async Task<IActionResult> OnGet()
	{
		var dbValues = await db.GameSystemFrameRates
			.Where(sf => sf.Id == Id)
			.Select(sf => new
			{
				sf.System!.Id,
				sf.System!.Code,
				sf.FrameRate,
				sf.RegionCode,
				sf.Preliminary,
				sf.Obsolete
			})
			.SingleOrDefaultAsync();

		if (dbValues is null)
		{
			return NotFound();
		}

		var parseResult = Enum.TryParse<RegionType>(dbValues.RegionCode, out var regionCode);

		FrameRate = new FrameRateEdit
		{
			SystemId = dbValues.Id,
			SystemCode = dbValues.Code,
			FrameRate = dbValues.FrameRate,
			RegionCode = parseResult ? regionCode : RegionType.Unknown,
			Preliminary = dbValues.Preliminary,
			Obsolete = dbValues.Obsolete
		};

		await PopulateUsages();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var frameRate = await db.GameSystemFrameRates
			.Include(sf => sf.System)
			.SingleOrDefaultAsync(sf => sf.Id == Id);

		if (frameRate is null)
		{
			return NotFound();
		}

		frameRate.FrameRate = FrameRate.FrameRate;
		frameRate.RegionCode = FrameRate.RegionCode.ToString();
		frameRate.Preliminary = FrameRate.Preliminary;
		frameRate.Obsolete = FrameRate.Obsolete;

		var displayName = $"{FrameRate.SystemCode} {FrameRate.RegionCode} {FrameRate.FrameRate.ToString(CultureInfo.InvariantCulture)}";
		SetMessage(
			await db.TrySaveChanges(),
			$"FrameRate {displayName} updated.",
			$"Unable to update {displayName} due to an unknown error");
		await gameSystemService.FlushCache();
		return BasePageRedirect("Edit", new { Id = FrameRate.SystemId });
	}

	public async Task<IActionResult> OnPostDelete(int systemId)
	{
		var frameRate = await db.GameSystemFrameRates
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

		db.GameSystemFrameRates.Remove(frameRate);
		SetMessage(await db.TrySaveChanges(), $"FrameRate {Id} deleted", $"Unable to delete FrameRate {Id}");

		return BasePageRedirect("Edit", new { Id = systemId });
	}

	private async Task PopulateUsages()
	{
		PublicationEntries = await db.Publications
			.Where(p => p.SystemFrameRateId == Id)
			.Select(p => new UsageEntry(p.Id, p.Title))
			.ToListAsync();

		SubmissionEntries = await db.Submissions
			.Where(s => s.SystemFrameRateId == Id)
			.Select(s => new UsageEntry(s.Id, s.Title))
			.ToListAsync();
	}

	public class FrameRateEdit
	{
		public int SystemId { get; init; }
		public string SystemCode { get; init; } = "";
		public double FrameRate { get; init; }

		public RegionType RegionCode { get; init; }
		public bool Preliminary { get; init; }

		public bool Obsolete { get; init; }
	}

	public record UsageEntry(int Id, string Title);
}
