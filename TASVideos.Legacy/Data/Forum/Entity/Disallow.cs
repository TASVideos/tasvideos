using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Forum.Entity
{
	public class Disallow
	{
		[Key]
		[Column("disallow_id")]
		public int Id { get; set; }

		[Column("disallow_username")]
		public string DisallowUserName { get; set; } = "";
	}
}
