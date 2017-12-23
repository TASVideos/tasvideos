using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity
{
	public class WikiPage : BaseEntity, ISoftDeletable
	{
		public int Id { get; set; }

		[Required]
		public string PageName { get; set; }

		public string Markup { get; set; }
		public int Revision { get; set; } = 1;

		public bool MinorEdit { get; set; }
		public string RevisionMessage { get; set; }

		public virtual WikiPage Child { get; set; } // The latest revision of a page is one with Child = null

		public bool IsDeleted { get; set; }
	}

	public static class ActiveQueryableExtensions
	{
		public static IQueryable<WikiPage> ThatAreCurrentRevisions(this IQueryable<WikiPage> list)
		{
			return list.Where(wp => wp.Child == null);
		}
	}
}
