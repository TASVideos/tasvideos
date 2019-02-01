using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly UserFileTasks _userFileTasks;
		private readonly ApplicationDbContext _db;

		public IndexModel(UserFileTasks userFileTasks, ApplicationDbContext db)
		{
			_userFileTasks = userFileTasks;
			_db = db;
		}

		public UserFileIndexModel Data { get; set; } = new UserFileIndexModel();

		public async Task OnGet()
		{
			Data = new UserFileIndexModel
			{
				UsersWithMovies = await _db.UserFiles
					.Where(uf => !uf.Hidden)
					.GroupBy(gkey => gkey.Author.UserName, gvalue => gvalue.UploadTimestamp).Select(
						uf => new UserFileIndexModel.UserWithMovie { UserName = uf.Key, Latest = uf.Max() })
					.ToListAsync(),
				LatestMovies = await _userFileTasks.GetLatest(10),
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
