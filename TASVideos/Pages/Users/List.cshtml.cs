﻿using System.ComponentModel;
using TASVideos.Core;

namespace TASVideos.Pages.Users;

[AllowAnonymous]
public class ListModel(ApplicationDbContext db, ICacheService cache) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<UserEntry> Users { get; set; } = PageOf<UserEntry>.Empty();

	public async Task OnGet()
	{
		if (string.IsNullOrWhiteSpace(Search.Sort))
		{
			Search.Sort = $"-{nameof(UserEntry.CreateTimestamp)}";
		}

		Users = await db.Users
			.Select(u => new UserEntry
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

		var exists = await db.Users.Exists(userName);
		return new JsonResult(exists);
	}

	private async ValueTask<IEnumerable<string>> GetUsersByPartial(string partialUserName)
	{
		var upper = partialUserName.ToUpper();
		var cacheKey = nameof(GetUsersByPartial) + upper;

		if (cache.TryGetValue(cacheKey, out List<string> list))
		{
			return list;
		}

		list = await db.Users
			.ThatPartiallyMatch(upper)
			.Select(u => u.UserName)
			.ToListAsync();

		cache.Set(cacheKey, list, Durations.OneMinuteInSeconds);

		return list;
	}

	public class UserEntry
	{
		[Sortable]
		public int Id { get; init; }

		[DisplayName("User Name")]
		[Sortable]
		public string? UserName { get; init; }

		public List<string> Roles { get; init; } = [];

		[DisplayName("Created")]
		[Sortable]
		public DateTime CreateTimestamp { get; init; }

		// Dummy to generate column header
		public object? Actions { get; init; }
	}
}
