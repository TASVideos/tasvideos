using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Roles.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Roles
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db, UserManager userManager)
			: base(userManager)
		{
			_db = db;
		}

		public RoleDisplayModel Role { get; set; }

		public async Task<IActionResult> OnGet(string role)
		{
			if (string.IsNullOrWhiteSpace(role))
			{
				return RedirectToAction("List");
			}

			Role = await _db.Roles
				.ProjectTo<RoleDisplayModel>()
				.Where(r => r.Name == role)
				.SingleOrDefaultAsync();

			if (Role == null)
			{
				return NotFound();
			}

			return Page();
		}
	}
}
