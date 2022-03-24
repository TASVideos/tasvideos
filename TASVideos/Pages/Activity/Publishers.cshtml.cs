using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Activity.Model;

namespace TASVideos.Pages.Activity;

[AllowAnonymous]
public class PublishersModel : PageModel
{
	private readonly ApplicationDbContext _db;

	public PublishersModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public IEnumerable<MovieEntryModel> Publications { get; set; } = new List<MovieEntryModel>();

	[FromRoute]
	public string UserName { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		if (string.IsNullOrWhiteSpace(UserName))
		{
			return NotFound();
		}

		var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
		if (user is null)
		{
			return NotFound();
		}

		Publications = await _db.Publications
			.ThatHaveBeenPublishedBy(user.Id)
			.Where(p => p.Submission!.PublisherId == user.Id)
			.Select(s => new MovieEntryModel
			{
				Id = s.Id,
				Title = s.Title
			})
			.ToListAsync();

		return Page();
	}
}
