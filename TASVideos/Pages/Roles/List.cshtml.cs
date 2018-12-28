using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Roles
{
	[AllowAnonymous]
	public class ListModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public ListModel(ApplicationDbContext db, UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
		}

		public IEnumerable<RoleDisplayModel> Roles { get; set; }

		public async Task OnGet(string role)
		{
			Roles = await _db.Roles
				.ProjectTo<RoleDisplayModel>()
				.ToListAsync();
		}
	}
}
