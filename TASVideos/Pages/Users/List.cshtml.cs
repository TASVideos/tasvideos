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
			Search.Sort = $"-{nameof(UserEntry.Created)}";
		}

		Users = await db.Users
			.Select(u => new UserEntry
			{
				Id = u.Id,
				UserName = u.UserName,
				Created = u.CreateTimestamp,
				Roles = u.UserRoles.Select(ur => ur.Role!.Name)
			})
			.SortedPageOf(Search);
	}

	public async Task<IActionResult> OnGetSearch(string partial)
	{
		if (string.IsNullOrWhiteSpace(partial) || partial.Length <= 2)
		{
			return Json(new List<string>());
		}

		var matches = await GetUsersByPartial(partial);
		return Json(matches);
	}

	public async Task<IActionResult> OnGetVerifyUniqueUserName(string userName)
	{
		if (string.IsNullOrWhiteSpace(userName))
		{
			return Json(false);
		}

		var exists = await db.Users.Exists(userName);
		return Json(exists);
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

		[Sortable]
		public string? UserName { get; init; }

		public IEnumerable<string> Roles { get; init; } = [];

		[Sortable]
		public DateTime Created { get; init; }
	}
}
