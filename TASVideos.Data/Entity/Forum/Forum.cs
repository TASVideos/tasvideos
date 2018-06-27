using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

		[StringLength(10)]
		public string ShortName { get; set; }

		public string Description { get; set; }
		public int Ordinal { get; set; }

		public bool Restricted { get; set; }
	}
}
