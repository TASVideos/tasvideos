using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity
{
	public class WikiPage : BaseEntity, ISoftDeletable
	{
		public int Id { get; set; }

		[Required]
		[StringLength(250)]
		public string PageName { get; set; }

		public string Markup { get; set; }
		public int Revision { get; set; } = 1;

		public bool MinorEdit { get; set; }

		[StringLength(1000)]
		public string RevisionMessage { get; set; }

		public int? ChildId { get; set; }
		public virtual WikiPage Child { get; set; } // The latest revision of a page is one with Child = null

		public bool IsDeleted { get; set; }
	} 

	public static class WikiQueryableExtensions
	{
		public static IQueryable<WikiPage> WithNoChildren(this IQueryable<WikiPage> list)
		{
			return list.Where(wp => wp.Child == null);
		}

		public static IQueryable<WikiPage> ForPage(this IQueryable<WikiPage> list, string pageName)
		{
			return list.Where(w => w.PageName == pageName);
		}

		public static IQueryable<WikiPage> Revision(this IQueryable<WikiPage> list, string pageName, int revision)
		{
			return list.Where(w => w.PageName == pageName && w.Revision == revision);
		}

		public static IQueryable<WikiPage> ExcludingMinorEdits(this IQueryable<WikiPage> list)
		{
			return list.Where(w => !w.MinorEdit);
		}

		public static bool IsCurrent(this WikiPage wikiPage)
		{
			return wikiPage != null
				&& wikiPage.ChildId == null
				&& wikiPage.IsDeleted == false;
		}
	}
}
