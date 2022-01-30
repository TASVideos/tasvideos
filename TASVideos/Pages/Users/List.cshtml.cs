using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Users.Models;

namespace TASVideos.Pages.Users;

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
	public PagingModel Search { get; set; } = new();

	public PageOf<UserListModel> Users { get; set; } = PageOf<UserListModel>.Empty();

	public async Task OnGet()
	{
		if (string.IsNullOrWhiteSpace(Search.Sort))
		{
			Search.Sort = $"-{nameof(UserListModel.CreateTimestamp)}";
		}

		Users = await _db.Users
			.Select(u => new UserListModel
			{
				Id = u.Id,
				UserName = u.UserName,
				CreateTimestamp = u.CreateTimestamp,
				Roles = u.UserRoles
					.Select(ur => ur.Role!.Name)
					.ToList()
			})
			.SortedPageOf(Search);
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

	private async ValueTask<IEnumerable<string>> GetUsersByPartial(string partialUserName)
	{
		var upper = partialUserName.ToUpper();
		var cacheKey = nameof(GetUsersByPartial) + upper;

		if (_cache.TryGetValue(cacheKey, out List<string> list))
		{
			return list;
		}

		list = await _db.Users
			.ThatPartiallyMatch(upper)
			.Select(u => u.UserName)
			.ToListAsync();

		_cache.Set(cacheKey, list, Durations.OneMinuteInSeconds);

		return list;
	}
}
