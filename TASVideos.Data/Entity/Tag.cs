using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity
{
	public class Tag
	{
		public int Id { get; set; }

		[Required]
		[StringLength(25)]
		public string Code { get; set; }

		[Required]
		[StringLength(50)]
		public string DisplayName { get; set; }

		public virtual ICollection<PublicationTag> PublicationTags { get; set; } = new List<PublicationTag>();
	}
}
