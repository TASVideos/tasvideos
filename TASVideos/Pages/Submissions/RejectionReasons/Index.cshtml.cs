namespace TASVideos.Pages.Submissions.RejectionReasons;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	public List<Rejection> Reasons { get; set; } = [];

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

		if (await db.SubmissionRejectionReasons.AnyAsync(r => r.DisplayName == displayName))
		{
			ModelState.AddModelError("displayName", $"{displayName} already exists");
			await Initialize();
			return Page();
		}

		db.SubmissionRejectionReasons.Add(new SubmissionRejectionReason
		{
			DisplayName = displayName
		});

		SetMessage(await db.TrySaveChanges(), $"reason: {displayName} created successfully", $"Unable to save reason: {displayName}");
		return BasePageRedirect("Index");
	}

	public async Task<IActionResult> OnPostDelete(int id)
	{
		if (!User.Has(PermissionTo.RejectionReasonMaintenance))
		{
			return AccessDenied();
		}

		var reason = await db.SubmissionRejectionReasons.SingleOrDefaultAsync(r => r.Id == id);
		if (reason is null)
		{
			return NotFound();
		}

		db.SubmissionRejectionReasons.Remove(reason);
		SetMessage(await db.TrySaveChanges(), $"reason: {reason.DisplayName} deleted successfully", $"Unable to delete reason: {reason.DisplayName}");

		return BasePageRedirect("Index");
	}

	private async Task Initialize()
	{
		Reasons = await db.SubmissionRejectionReasons
			.Select(r => new Rejection(r.Id, r.DisplayName, r.Submissions.Count(s => s.Status == SubmissionStatus.Rejected)))
			.ToListAsync();
	}

	public record Rejection(int Id, string Reason, int SubmissionCount);
}
