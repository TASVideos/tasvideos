using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.ViewComponents.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.NoEmulator)]
	public class NoEmulator : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public NoEmulator(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var model = new MissingRomModel
			{
				Publications = await _db.Publications
					.Where(p => string.IsNullOrEmpty(p.EmulatorVersion))
					.OrderBy(p => p.Id)
					.Select(p => new MissingRomModel.Entry(p.Id, p.Title))
					.ToListAsync(),
				Submissions = await _db.Submissions
					.Where(s => string.IsNullOrEmpty(s.EmulatorVersion))
					.OrderBy(s => s.Id)
					.Select(s => new MissingRomModel.Entry(s.Id, s.Title))
					.ToListAsync()
			};
			return View(model);
		}
	}
}
