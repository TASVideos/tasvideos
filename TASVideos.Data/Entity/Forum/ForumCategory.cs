using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity.Forum
{
	public class ForumCategory : BaseEntity
	{
		public int Id { get; set; }
		public virtual ICollection<Forum> Forums { get; set; } = new HashSet<Forum>();

		[Required]
		[StringLength(30)]
		public string Title { get; set; }

		public int Ordinal { get; set; }

		public string Description { get; set; }
	}
}
