using TASVideos.Data.Entity.Awards;

namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class ListCategoryModel(ApplicationDbContext db, IMediaFileUploader mediaFileUploader) : BasePageModel
{
	public List<AwardCategoryEntry> Categories { get; set; } = [];

	public async Task OnGet()
	{
		Categories = await db.Awards
			.Select(a => new AwardCategoryEntry(
				a.Id,
				a.Type,
				a.ShortName,
				a.Description,
				db.PublicationAwards.Any(pa => pa.AwardId == a.Id)
					|| db.UserAwards.Any(ua => ua.AwardId == a.Id)))
			.ToListAsync();
	}

	public async Task<IActionResult> OnPostDelete(int id)
	{
		var awardCategory = await db.Awards
			.Where(a => a.Id == id)
			.Select(a => new
			{
				a.ShortName,
				InUse = db.PublicationAwards.Any(pa => pa.AwardId == a.Id)
					|| db.UserAwards.Any(ua => ua.AwardId == a.Id)
			})
			.SingleOrDefaultAsync();
		if (awardCategory is null)
		{
			return NotFound();
		}

		if (awardCategory.InUse)
		{
			return BadRequest("Cannot delete an award category that is in use.");
		}

		db.Awards.Attach(new Award { Id = id }).State = EntityState.Deleted;
		await db.SaveChangesAsync();

		mediaFileUploader.DeleteAwardImage(awardCategory.ShortName);

		return BasePageRedirect("ListCategories");
	}

	public record AwardCategoryEntry(int Id, AwardType Type, string ShortName, string Description, bool InUse);
}
