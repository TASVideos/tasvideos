using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.Pages.Diagnostics
{
	[RequirePermission(PermissionTo.SeeDiagnostics)]
	public class WikiSyntaxErrorsModel : PageModel
	{
		private readonly ApplicationDbContext _db;

		public WikiSyntaxErrorsModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public class Row
		{
			public string PageName { get; set; }
			public string Markup { get; set; }
			public int ErrorLocation { get; set; }
			public string ErrorMessage { get; set; }
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

		public List<Row> Rows { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var pages = await _db.WikiPages
				.ThatAreCurrentRevisions()
				.ThatAreNotDeleted()
				.Select(p => new
				{
					PageName = p.PageName,
					Markup = p.Markup
				})
				.ToListAsync();
			var rows = pages
				.Select(p => new
				{
					p,
					err = Util.ParsePageForErrors(p.Markup)
				})
				.Where(a => a.err != null)
				.Select(a => new Row
				{
					PageName = a.p.PageName,
					Markup = a.p.Markup,
					ErrorLocation = a.err.TextLocation,
					ErrorMessage = a.err.Message
				})
				.ToList();

			Rows = rows;
			return Page();
		}
	}
}
