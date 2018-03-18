using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class Categories
	{
		[Key]
		[Column("cat_id")]
		public int Id { get; set; }

		[Column("cat_title")]
		public string Title { get; set; }

		[Column("cat_order")]
		public int Order { get; set; }

		[Column("cat_desc")]
		public string Description { get; set; }
	}
}
