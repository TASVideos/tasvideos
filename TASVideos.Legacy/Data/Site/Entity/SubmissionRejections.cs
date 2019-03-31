using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
	public class SubmissionRejections
	{
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("reason")]
		public int Reason { get; set; }
	}
}
