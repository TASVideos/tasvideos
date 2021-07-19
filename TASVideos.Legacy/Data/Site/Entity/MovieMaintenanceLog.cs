using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class MovieMaintenanceLog
	{
		[Column("id")]
		public int Id { get; set; }

		[Column("movieid")]
		public int MovieId { get; set; }

		[Column("userid")]
		public int UserId { get; set; }

		[Column("timestamp")]
		public int TimeStamp { get; set; }

		[Column("type")]
		public string Type { get; set; } = "";

		[Column("addremove")]
		public string AddRemove { get; set; } = "";

		[Column("content")]
		public string Content { get; set; } = "";
	}
}
