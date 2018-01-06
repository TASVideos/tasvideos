using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.Entity
{
    public class Submission : BaseEntity
    {
		public int Id { get; set; }
		public virtual WikiPage WikiContent { get; set; }
		public virtual User Submitter { get; set; }

		public virtual ICollection<SubmissionAuthor> SubmissionAuthors { get; set; } = new HashSet<SubmissionAuthor>();

		// TODO: game id
		// TODO: intended tier

		public virtual User Judge { get; set; }
		public virtual User Publisher { get; set; }

		public SubmissionStatus Status { get; set; } = SubmissionStatus.New;
		public virtual ICollection<SubmissionStatusHistory> History { get; set; } = new HashSet<SubmissionStatusHistory>();

		// TODO: we eventually should want to move these to the file server instead
		public byte[] MovieFile { get; set; }

		// Metadata parsed from movie file
		public virtual GameSystem System { get; set; }
		public virtual GameSystemFrameRate SystemFrameRate { get; set; }

		public int Frames { get; set; }
		public int RerecordCount { get; set; }

		// Metadata, user entered

		// TODO: additional non-registered authors field

		[StringLength(100)]
		public string EncodeEmbedLink { get; set; }

		[StringLength(20)]
		public string GameVersion { get; set; }

		[StringLength(100)]
		public string GameName { get; set; }

		[StringLength(50)]
		public string Branch { get; set; }

		[StringLength(100)]
		public string RomName { get; set; }
		
		[StringLength(50)]
		public string EmulatorVersion { get; set; }
	}

	public enum SubmissionStatus
	{
		[Display(Name = "New")]
		New,

		[Display(Name = "Delayed")]
		Delayed,

		[Display(Name = "Needs More Info")]
		NeedsMoreInfo,

		[Display(Name = "Judging Underway")]
		JudgingUnderWay,

		[Display(Name = "Accepted")]
		Accepted,

		[Display(Name = "Publication Underway")]
		PublicationUnderway,

		[Display(Name = "Published")]
		Published,

		[Display(Name = "Rejected")]
		Rejected,

		[Display(Name = "Cancelled")]
		Cancelled
	}
}
