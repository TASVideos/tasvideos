using System.Diagnostics;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Models;
using TASVideos.Data;

namespace TASVideos.Controllers
{
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _db;

		public HomeController(ApplicationDbContext db)
		{
			_db = db;
		}

		public IActionResult Index()
		{
			var allRoles = _db.Roles
				.Include(r => r.RolePermission)
				.ToList();

			ViewData["Users"] = _db.Users.ToList();
			return View(allRoles);
		}

		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
