using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class GameName
	{
		[Key]
		[Column("gn_id")]
		public int Id { get; set; }

		[Column("goodname")]
		public string GoodName { get; set; }

		[Column("displayname")]
		public string DisplayName { get; set; }

		[Column("abbreviation")]
		public string Abbreviation { get; set; }

		[Column("sys_id")]
		public int SystemId { get; set; }

		[Column("resource_page")]
		public string ResourceName { get; set; }

		[Column("searchkey")]
		public string SearchKey { get; set; }

		[Column("youtube_tags")]
		public string YoutubeTags { get; set; }
	}
}
