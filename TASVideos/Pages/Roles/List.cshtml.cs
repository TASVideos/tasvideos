using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.RazorPages.Pages.Roles.Models;

namespace TASVideos.RazorPages.Pages.Roles
{
	[AllowAnonymous]
	public class ListModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public ListModel(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		public IEnumerable<RoleDisplayModel> Roles { get; set; } = new List<RoleDisplayModel>();

		public async Task OnGet()
		{
			Roles = await _mapper
				.ProjectTo<RoleDisplayModel>(_db.Roles)
				.ToListAsync();
		}
	}
}
