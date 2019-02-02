using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class ForUserModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public ForUserModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public string UserName { get; set; }

		public IEnumerable<UserFileModel> Files { get; set; } = new List<UserFileModel>();

		public async Task OnGet()
		{
			Files = await _db.UserFiles
				.ForAuthor(UserName)
				.FilterByHidden(includeHidden: false)
				.ProjectTo<UserFileModel>()
				.ToListAsync();
		}
	}
}
