using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tags
{
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();

		public async Task OnGet()
		{
			Tags = await _db.Tags.ToListAsync();
		}
	}
}
