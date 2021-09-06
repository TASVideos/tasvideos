using System.ComponentModel.DataAnnotations.Schema;
namespace TASVideos.Legacy.Data.Site.Entity
{
	public class UserMaintenanceLogs
	{
		[Column("id")]
		public int Id { get; set; }

		[Column("userid")]
		public int UserId { get; set; }

		[Column("editorid")]
		public int EditorId { get; set; }

		[Column("timestamp")]
		public int TimeStamp { get; set; }

		[Column("type")]
		public string Type { get; set; } = "";

		[Column("content")]
		public string Content { get; set; } = "";
	}
}
