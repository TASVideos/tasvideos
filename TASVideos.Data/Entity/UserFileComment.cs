using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity
{
	public class UserFileComment
	{
		[Key]
		public int Id { get; set; }

		public long UserFileId { get; set; }
		public virtual UserFile UserFile { get; set; }

		[StringLength(255)]
		public string Ip { get; set; }

		public int? ParentId { get; set; }
		public virtual UserFileComment Parent { get; set; }

		[StringLength(255)]
		public string Title { get; set; }

		public string Text { get; set; }

		public DateTime CreationTimeStamp { get; set; }

		public int UserId { get; set; }
		public virtual User User { get; set; }

		public virtual ICollection<UserFileComment> Children { get; set; } = new HashSet<UserFileComment>();
	}
}
