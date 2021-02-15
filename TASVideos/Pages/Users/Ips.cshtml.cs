using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Users
{
	[RequirePermission(PermissionTo.ViewPrivateUserData)]
	public class IpsModel : PageModel
	{
		private readonly ApplicationDbContext _db;

		public IpsModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public string UserName { get; set; } = "";

		public ICollection<string> Ips { get; set; } = new List<string>();

		public async Task<IActionResult> OnGet()
		{
			var user = await _db.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
			if (user is null)
			{
				return NotFound();
			}

			Ips = await _db.ForumPosts
				.Where(p => p.PosterId == user.Id)
				.Where(p => p.IpAddress != null)
				.Select(p => p.IpAddress ?? "")
				.Distinct()
				.ToListAsync();

			return Page();
		}
	}
}
