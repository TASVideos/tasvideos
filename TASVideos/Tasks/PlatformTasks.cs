using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;

namespace TASVideos.Tasks
{
    public class PlatformTasks
	{
		private readonly ApplicationDbContext _db;

		public PlatformTasks(ApplicationDbContext db)
		{
			_db = db;
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
