using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Data;

namespace TASVideos.Pages.Logs;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromQuery]
	public LogPaging Search { get; set; } = new();

	public PageOf<LogEntry> History { get; set; } = PageOf<LogEntry>.Empty();

	[FromRoute]
	public string Table { get; set; } = "";

	[FromRoute]
	public int? RowId { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var query = _db.AutoHistory
			.GroupJoin(_db.Users, outerKey => outerKey.UserId, innerKey => innerKey.Id, (h, user) => new { h, user })
			.SelectMany(g => g.user.DefaultIfEmpty(), (g, user) => new LogEntry
			{
				RowId = g.h.RowId,
				UserName = user == null ? "Unknown_User" : user.UserName,
				Created = g.h.Created,
				TableName = g.h.TableName,
				Changed = g.h.Changed,
				Kind = g.h.Kind,
			});

		if (!string.IsNullOrWhiteSpace(Table))
		{
			query = query.Where(h => h.TableName == Table);
		}

		if (RowId.HasValue)
		{
			var rowStr = RowId.Value.ToString();
			query = query.Where(h => h.RowId == rowStr);
		}

		History = await query.SortedPageOf(Search);
		return Page();
	}

	public class LogPaging : PagingModel
	{
		public LogPaging()
		{
			Sort = $"-{nameof(LogEntry.Created)}";
			PageSize = 25;
		}
	}

	public class LogEntry
	{
		[Sortable]
		public string RowId { get; init; } = "";

		[Sortable]
		public string UserName { get; init; } = "";

		[Sortable]
		public DateTime Created { get; init; } = DateTime.UtcNow;

		[Sortable]
		public string TableName { get; init; } = "";

		[Sortable]
		public EntityState Kind { get; init; }

		public string Changed { get; init; } = "";
	}

	public class ModifiedEntry
	{
		[JsonPropertyName("before")]
		public object Before { get; set; } = "";

		[JsonPropertyName("after")]
		public object After { get; set; } = "";
	}
}
