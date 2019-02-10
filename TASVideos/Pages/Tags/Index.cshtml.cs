using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tags
{
	[RequirePermission(PermissionTo.TagMaintenance)]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[TempData]
		public string Message { get; set; }

		[TempData]
		public string MessageType { get; set; }

		public bool ShowMessage => !string.IsNullOrWhiteSpace(Message);

		public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();

		public async Task OnGet()
		{
			Tags = await _db.Tags
				.OrderBy(t => t.Code)
				.ToListAsync();
		}
	}
}
