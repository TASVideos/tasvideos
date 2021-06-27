using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.AviEncodes)]
	public class AviEncodes : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public AviEncodes(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var encodes = await _db.PublicationFiles
				.Where(pf => pf.Path.EndsWith(".avi.torrent"))
				.Select(pf => new AviEncodeResultModel(pf.PublicationId, pf.Publication!.Title, pf.Path))
				.ToListAsync();

			return View(encodes);
		}
	}
}
