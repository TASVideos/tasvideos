using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity.Forum
{
	public class Forum : BaseEntity
	{
		public int Id { get; set; }

		public int CategoryId { get; set; }
		public virtual ForumCategory Category { get; set; }

		public virtual ICollection<ForumTopic> ForumTopics { get; set; } = new HashSet<ForumTopic>();

		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		[Required]
		[StringLength(10)]
		public string ShortName { get; set; }

		[StringLength(1000)]
		public string Description { get; set; }

		public int Ordinal { get; set; }

		public bool Restricted { get; set; }
	}

	public static class ForumQueryableExtensions
	{
		public static IQueryable<Forum> ExcludeRestricted(this IQueryable<Forum> list, bool seeRestricted)
		{
			return list.Where(f => seeRestricted || !f.Restricted);
		}
	}
}
