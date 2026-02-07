using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NpgsqlTypes;

namespace TASVideos.Data.Entity;

public class WikiPage : BaseEntity, ISoftDeletable
{
	public int Id { get; set; }

	public string PageName { get; set; } = "";

	public string Markup { get; set; } = "";

	public int Revision { get; set; } = 1;

	public bool MinorEdit { get; set; }

	public string? RevisionMessage { get; set; }

	public int? ChildId { get; set; }
	public WikiPage? Child { get; set; } // The latest revision of a page is one with Child = null

	public bool IsDeleted { get; set; }

	[JsonIgnore]
	public NpgsqlTsVector SearchVector { get; set; } = null!;

	public int? AuthorId { get; set; }
	public User? Author { get; set; }

	public bool IsCurrent() => !ChildId.HasValue && !IsDeleted;
}

public static class WikiQueryableExtensions
{
	/// <param name="list">The query to filter.</param>
	extension(IQueryable<WikiPage> list)
	{
		public IQueryable<WikiPage> ThatAreCurrent() => list.Where(wp => wp.ChildId == null);

		public IQueryable<WikiPage> ThatAreNotCurrent() => list.Where(wp => wp.ChildId != null);

		/// <summary>
		/// Filters to pages at a specific indentation level
		/// Foo = 1
		/// Foo/Bar = 2
		/// Foo/Bar/Baz = 3
		/// </summary>
		public IQueryable<WikiPage> ForPageLevel(int indentationLevel)
		{
			var slashCount = indentationLevel - 1;
			return list.Where(wp => Regex.IsMatch(wp.PageName, $@"^[^\/]+(\/[^\/]+){{{slashCount}}}$"));
		}

		public IQueryable<WikiPage> ForPage(string pageName) => list.Where(w => w.PageName == pageName);

		public IQueryable<WikiPage> Revision(string pageName, int revision)
			=> list.Where(w => w.PageName == pageName && w.Revision == revision);

		public IQueryable<WikiPage> ExcludingMinorEdits() => list.Where(w => !w.MinorEdit);

		public IQueryable<WikiPage> CreatedBy(string userName) => list.Where(t => t.Author!.UserName == userName);

		/// <summary>
		/// Filters the list of wiki pages to only pages that are nest beneath the given page.
		/// If no pageName is provided, then a master list of subpages is provided
		/// ex: /Foo/Bar, /Foo/Bar2 and /Foo/Bar/Baz are all subpages of /Foo.
		/// </summary>
		/// <seealso cref="WikiPage"/>
		/// <param name="pageName">The name of the page to get Subpages from.</param>
		public IQueryable<WikiPage> ThatAreSubpagesOf(string? pageName)
		{
			pageName = (pageName ?? "").Trim('/');
			list = list
				.ThatAreNotDeleted()
				.ThatAreCurrent()
				.Where(wp => wp.PageName != pageName);

			if (!string.IsNullOrWhiteSpace(pageName))
			{
				list = list.Where(wp => wp.PageName.StartsWith(pageName + "/"));
			}

			return list;
		}

		/// <summary>
		/// Filters the list of wiki pages to only pages that are parents of the given page
		/// ex: /Foo is a parent of /Foo/Bar
		/// ex: /Foo and /Foo/Bar are parents of /Foo/Bar/Baz.
		/// </summary>
		/// <seealso cref="WikiPage"/>
		/// <param name="pageName">The name of the page to get parent pages from.</param>
		public IQueryable<WikiPage> ThatAreParentsOf(string? pageName)
		{
			pageName = (pageName ?? "").Trim('/');
			if (string.IsNullOrWhiteSpace(pageName)
				|| !pageName.Contains('/')) // Easy optimization, pages without a / have no parents
			{
				return Enumerable.Empty<WikiPage>().AsQueryable();
			}

			return list
				.ThatAreNotDeleted()
				.ThatAreCurrent()
				.Where(wp => wp.PageName != pageName)
				.Where(wp => pageName.StartsWith(wp.PageName + "/"));
		}

		public IQueryable<WikiPage> WebSearch(string searchTerms)
			=> list.Where(w => w.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(searchTerms)));

		public IOrderedQueryable<WikiPage> ByWebRanking(string searchTerms)
			=> list.OrderByDescending(p => p.SearchVector.Rank(EF.Functions.WebSearchToTsQuery(searchTerms)));
	}
}
