using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Roles.Models;
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

		[TempData]
		public string Message { get; set; }

		[TempData]
		public string MessageType { get; set; }

		public bool ShowMessage => !string.IsNullOrWhiteSpace(Message);

		public IEnumerable<RoleDisplayModel> Roles { get; set; }

		public async Task OnGet(string role)
		{
			Roles = await _db.Roles
				.ProjectTo<RoleDisplayModel>()
				.ToListAsync();
		}
	}
}
