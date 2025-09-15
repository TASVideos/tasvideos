using System.Text.Json.Serialization;

namespace TASVideos.Pages.Logs;

[AllowAnonymous]
public class IndexModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public LogPaging Search { get; set; } = new();

	public PageOf<LogEntry, LogPaging> History { get; set; } = new([], new());

	[FromRoute]
	public string Table { get; set; } = "";

	[FromRoute]
	public int? RowId { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var query = db.AutoHistory
			.GroupJoin(db.Users, outerKey => outerKey.UserId, innerKey => innerKey.Id, (h, user) => new { h, user })
			.SelectMany(g => g.user.DefaultIfEmpty(), (g, user) => new LogEntry
			{
				RowId = g.h.RowId,
				UserName = user == null ? "Unknown_User" : user.UserName,
				Created = g.h.Created,
				TableName = g.h.TableName,
				Changed = g.h.Changed ?? "",
				Kind = g.h.Kind
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

	[PagingDefaults(Sort = $"-{nameof(LogEntry.Created)}")]
	public class LogPaging : PagingModel;

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
