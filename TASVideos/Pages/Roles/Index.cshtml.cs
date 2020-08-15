using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Roles.Models;

namespace TASVideos.Pages.Roles
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public RoleDisplayModel Role { get; set; } = new RoleDisplayModel();

		public async Task<IActionResult> OnGet(string role)
		{
			if (string.IsNullOrWhiteSpace(role))
			{
				return RedirectToPage("/Roles/List");
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
