using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class WikiSyntaxErrorsModel : PageModel
{
	private readonly IWikiPages _wikiPages;

	public WikiSyntaxErrorsModel(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	public class Row
	{
		public string PageName { get; set; } = "";
		public string Markup { get; set; } = "";
		public int ErrorLocation { get; set; }
		public string? ErrorMessage { get; set; }
		public string ExcerptBefore
		{
			get
			{
				var from = Math.Max(ErrorLocation - 20, 0);
				var to = ErrorLocation;
				return Markup.Substring(from, to - from);
			}
		}

		public string ExcerptAfter
		{
			get
			{
				var from = ErrorLocation;
				var to = Math.Min(ErrorLocation + 20, Markup.Length);
				return Markup.Substring(from, to - from);
			}
		}
	}

	public ICollection<Row> Rows { get; set; } = new List<Row>();

	public async Task<IActionResult> OnGet()
	{
		var pages = await _wikiPages.Query
			.ThatAreNotDeleted()
			.WithNoChildren()
			.Select(p => new
			{
				p.PageName,
				p.Markup
			})
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

		return Page();
	}
}
