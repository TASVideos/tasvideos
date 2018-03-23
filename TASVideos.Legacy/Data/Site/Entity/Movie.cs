using System.Collections.Generic;
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
		public virtual User Player { get; set; }

		[Column("gameid")]
		public int GameId { get; set; } = 0;

		[Column("nickname")]
		public string Branch { get; set; }

		[Column("gameversion")]
		public string GameVersion { get; set; }

		[Column("systemid")]
		public int SystemId { get; set; }

		[Column("lastchange")]
		public int LastChange { get; set; }

		[Column("submissionid")]
		public int SubmissionId { get; set; } = -1;
		public virtual Submission Submission { get; set; }

		[Column("obsoleted_by")]
		public int? ObsoletedBy { get; set; }

		[Column("published_by")]
		public int PublisherId { get; set; }
		public virtual User Publisher { get; set; }

		[Column("pubdate")]
		public int PublishedDate { get; set; }

		[Column("tier")]
		public int Tier { get; set; } = 2;

		public ICollection<MovieFile> MovieFiles { get; set; } = new HashSet<MovieFile>();
		public ICollection<MovieClass> MovieClasses { get; set; } = new List<MovieClass>();
	}
}
