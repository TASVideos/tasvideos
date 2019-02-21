using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public IndexModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public UserFileIndexModel Data { get; set; }

		public async Task OnGet()
		{
			Data = new UserFileIndexModel
			{
				UsersWithMovies = await _db.UserFiles
					.Where(uf => !uf.Hidden)
					.GroupBy(gkey => gkey.Author.UserName, gvalue => gvalue.UploadTimestamp).Select(
						uf => new UserFileIndexModel.UserWithMovie { UserName = uf.Key, Latest = uf.Max() })
					.ToListAsync(),
				LatestMovies = await _db.UserFiles
					.ThatArePublic()
					.ByRecentlyUploaded()
					.ProjectTo<UserMovieListModel>()
					.Take(10)
					.ToListAsync(),
				GamesWithMovies = await _db.Games
					.Where(g => g.UserFiles.Any())
					.OrderBy(g => g.System.Code)
					.ThenBy(g => g.DisplayName)
					.Select(g => new UserFileIndexModel.GameWithMovie
					{
						GameId = g.Id,
						GameName = g.DisplayName,
						SystemCode = g.System.Code,
						Latest = g.UserFiles.Select(uf => uf.UploadTimestamp).Max()
					})
					.ToListAsync()
			};
		}
	}
}
