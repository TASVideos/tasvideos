using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class EditHistoryModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public EditHistoryModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public string UserName { get; set; }

		public UserWikiEditHistoryModel History { get; set; }

		public async Task OnGet()
		{
			History = new UserWikiEditHistoryModel
			{
				UserName = UserName,
				Edits = await _db.WikiPages
					.ThatAreNotDeleted()
					.CreatedBy(UserName)
					.ByMostRecent()
					.ProjectTo<UserWikiEditHistoryModel.EditEntry>()
					.ToListAsync()
			};
		}
	}
}
