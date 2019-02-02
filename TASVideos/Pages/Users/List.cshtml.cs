using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Pages.Users.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Users
{
	[AllowAnonymous]
	public class ListModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public ListModel(
			ApplicationDbContext db,
			ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		[FromQuery]
		public PagedModel Search { get; set; } = new PagedModel();

		public PageOf<UserListModel> Users { get; set; }

		public void OnGet()
		{
			Users = _db.Users
				.Select(u => new UserListModel
				{
					Id = u.Id,
					UserName = u.UserName,
					CreateTimeStamp = u.CreateTimeStamp,
					Roles = u.UserRoles
						.Select(ur => ur.Role.Name)
				})
				.SortedPageOf(_db, Search);
		}

		public async Task<IActionResult> OnGetSearch(string partial)
		{
			if (!string.IsNullOrWhiteSpace(partial) && partial.Length > 2)
			{
				var matches = await GetUsersByPartial(partial);
				return new JsonResult(matches);
			}

			return new JsonResult(new List<string>());
		}

		public async Task<IActionResult> OnGetVerifyUniqueUserName(string userName)
		{
			if (string.IsNullOrWhiteSpace(userName))
			{
				return new JsonResult(false);
			}

			var exists = await _db.Users.Exists(userName);
			return new JsonResult(exists);
		}

		private async Task<IEnumerable<string>> GetUsersByPartial(string partialUserName)
		{
			var upper = partialUserName.ToUpper();
			var cacheKey = nameof(GetUsersByPartial) + upper;

			if (_cache.TryGetValue(cacheKey, out List<string> list))
			{
				return list;
			}

			list = await _db.Users
				.Where(u => u.NormalizedUserName.Contains(upper))
				.Select(u => u.UserName)
				.ToListAsync();

			_cache.Set(cacheKey, list, Durations.OneMinuteInSeconds);

			return list;
		}
	}
}
