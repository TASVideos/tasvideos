using TASVideos.WikiEngine;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class WikiSyntaxErrorsModel(ApplicationDbContext db) : BasePageModel
{
	public List<Row> Rows { get; set; } = [];

	public async Task OnGet()
	{
		var pages = await db.WikiPages
			.ThatAreNotDeleted()
			.ThatAreCurrent()
			.Select(p => new { p.PageName, p.Markup })
			.ToListAsync();
		Rows = pages
			.Select(p => new
			{
				p,
				err = Util.ParsePageForErrors(p.Markup)
			})
			.Where(a => a.err is not null)
			.Select(a => new Row
			{
				PageName = a.p.PageName,
				Markup = a.p.Markup,
				ErrorLocation = a.err!.TextLocation,
				ErrorMessage = a.err.Message
			})
			.ToList();
	}

	public class Row
	{
		public string PageName { get; init; } = "";
		public string Markup { get; init; } = "";
		public int ErrorLocation { get; init; }
		public string? ErrorMessage { get; init; }
		public string ExcerptBefore
		{
			get
			{
				var from = Math.Max(ErrorLocation - 20, 0);
				var to = ErrorLocation;
				return Markup[from..to];
			}
		}

		public string ExcerptAfter
		{
			get
			{
				var from = ErrorLocation;
				var to = Math.Min(ErrorLocation + 20, Markup.Length);
				return Markup[from..to];
			}
		}
	}
}
