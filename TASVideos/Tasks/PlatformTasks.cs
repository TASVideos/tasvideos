using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.ViewComponents;

namespace TASVideos.Tasks
{
    public class PlatformTasks
	{
		private readonly ApplicationDbContext _db;

		public PlatformTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<PlatformFramerateModel>> GetAllPlatformFrameRates()
		{
			var query = from system in _db.GameSystems
				join frameRate in _db.GameSystemFrameRates on system.Id equals frameRate.GameSystemId
				select new PlatformFramerateModel
				{
					SystemCode = system.Code,
					FrameRate = frameRate.FrameRate,
					RegionCode = frameRate.RegionCode,
					Preliminary = frameRate.Preliminary
				};

			return await query
				.OrderBy(p => p.SystemCode)
				.ThenBy(p => p.RegionCode)
				.ToListAsync();
		}

		public async Task<IEnumerable<SelectListItem>> GetGameSystemDropdownList()
		{
			return await _db.GameSystems
				.Select(s => new SelectListItem
				{
					Text = s.Code,
					Value = s.Code
				})
				.ToListAsync();
		}
	}
}
