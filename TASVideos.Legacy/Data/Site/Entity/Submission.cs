using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TASVideos.Legacy.Data.Site.Entity
{
    public class Submission
    {
		[Key]
		[Column("id")]
		public int Id { get; set; }

		[Column("userid")]
		public int UserId { get; set; }
		public User User { get; set; }

		[Column("timestamp")]
		public int TimeStamp { get; set; }

		[Column("systemid")]
		public int SystemId { get; set; }

		[Column("gamename")]
		public string GameName { get; set; }

		[Column("gameversion")]
		public string GameVersion { get; set; }

		[Column("romname")]
		public string RomName { get; set; }

		[Column("authornick")]
		public string Author { get; set; }

		[Column("frames")]
		public int Frames { get; set; }

		[Column("length")]
		public decimal Length { get; set; }

		[Column("rerecords")]
		public int Rerecord { get; set; }

		[Column("alerts")]
		public string Alerts { get; set; }

		[Column("status")]
		public string Status { get; set; }

		[Column("statusby")]
		public string StatusBy { get; set; }

		[Column("content")]
		public byte[] Content { get; set; }

		[Column("subdate")]
		public int SubmissionDate { get; set; }

		[Column("judged_by")]
		public int JudgeId { get; set; }

		[Column("judgedate")]
		public int JudgeDate { get; set; }

		[Column("gn_id")]
		public int? GameNameId { get; set; }

		[Column("emuversion")]
		public string EmulatorVersion { get; set; }

		[Column("intended_tier")]
		public int? IntendedTier { get; set; }
	}
}
