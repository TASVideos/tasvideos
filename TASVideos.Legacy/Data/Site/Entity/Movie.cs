using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
    public class Movie
    {
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("playerid")]
		public int PlayerId { get; set; }

		[Column("gameid")]
		public int GameId { get; set; } = 0;

		[Column("systemid")]
		public int SystemId { get; set; }

		[Column("lastchange")]
		public int LastChange { get; set; }

		[Column("submissionid")]
		public int SubmissionId { get; set; } = -1;

		[Column("obsoleted_by")]
		public int? ObsoletedBy { get; set; }

		[Column("pubdate")]
		public int PublishedDate { get; set; }

		[Column("tier")]
		public int Tier { get; set; } = 2;
	}
}
