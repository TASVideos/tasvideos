using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class ReferrersModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		public ReferrersModel(
			ApplicationDbContext db,
			UserManager userManager) 
			: base(userManager)
		{
			_db = db;
		}

		[FromQuery]
		public string Path { get; set; }

		public IEnumerable<WikiPageReferral> Referrals { get; set; } = new List<WikiPageReferral>();

		public async Task OnGet()
		{
			Path = Path.Trim('/');
			Referrals = await _db.WikiReferrals
				.ThatReferTo(Path)
				.ToListAsync();
			ViewData["PageName"] = Path;
		}
	}
}
