﻿using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Pages.Roles.Models;

namespace TASVideos.Pages.Roles
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public IndexModel(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		public RoleDisplayModel Role { get; set; } = new ();

		public async Task<IActionResult> OnGet(string role)
		{
			if (string.IsNullOrWhiteSpace(role))
			{
				return RedirectToPage("/Roles/List");
			}

			Role = await _mapper
				.ProjectTo<RoleDisplayModel>(
					_db.Roles.Where(r => r.Name == role))
				.SingleOrDefaultAsync();

			if (Role == null)
			{
				return NotFound();
			}

			return Page();
		}
	}
}
