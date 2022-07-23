using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Submissions.RejectionReasons;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public record RejectionRecord(int Id, string Reason, int SubmissionCount);
	public IEnumerable<RejectionRecord> Reasons { get; set; } = new List<RejectionRecord>();

	public async Task OnGet()
	{
		await Initialize();
	}

	public async Task<IActionResult> OnPost(string displayName)
	{
		if (!User.Has(PermissionTo.RejectionReasonMaintenance))
		{
			return AccessDenied();
		}

		if (await _db.SubmissionRejectionReasons
				.AnyAsync(r => r.DisplayName == displayName))
		{
			ModelState.AddModelError("displayName", $"{displayName} already exists");
			await Initialize();
			return Page();
		}

		_db.SubmissionRejectionReasons.Add(new SubmissionRejectionReason
		{
			DisplayName = displayName
		});

		await ConcurrentSave(_db, $"reason: {displayName} created successfully", $"Unable to save reason: {displayName}");
		return BasePageRedirect("Index");
	}

	public async Task<IActionResult> OnPostDelete(int id)
	{
		if (!User.Has(PermissionTo.RejectionReasonMaintenance))
		{
			return AccessDenied();
		}

		var reason = await _db.SubmissionRejectionReasons.SingleOrDefaultAsync(r => r.Id == id);
		if (reason is null)
		{
			return NotFound();
		}

		_db.SubmissionRejectionReasons.Remove(reason);
		await ConcurrentSave(_db, $"reason: {reason.DisplayName} deleted successfully", $"Unable to delete reason: {reason.DisplayName}");

		return BasePageRedirect("Index");
	}

	private async Task Initialize()
	{
		Reasons = await _db.SubmissionRejectionReasons
			.Select(r => new RejectionRecord(r.Id, r.DisplayName, r.Submissions.Count))
			.ToListAsync();
	}
}
