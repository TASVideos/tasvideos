using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.Entity
{
    public class Publication : BaseEntity
    {
		public int Id { get; set; }

		public int GameId { get; set; }
		public virtual Game.Game Game { get; set; }

		public int SystemId { get; set; }
		public virtual GameSystem System { get; set; }

		public int SystemFrameRateId { get; set; }
		public virtual GameSystemFrameRate SystemFrameRate { get; set; }

		public int RomId { get; set; }
		public virtual GameRom Rom { get; set; }

		public int TierId { get; set; }
		public virtual Tier Tier { get; set; }

		public int SubmissionId { get; set; }
		public virtual Submission Submission { get; set; }
		public virtual ICollection<PublicationAuthor> Authors { get; set; } = new List<PublicationAuthor>();
		public virtual Publication ObsoletedBy { get; set; }

		public virtual WikiPage WikiContent { get; set; }

		// TODO: we eventually should want to move these to the file server instead
		[Required]
		public byte[] MovieFile { get; set; }

		[StringLength(50)]
		public string Branch { get; set; }

		[StringLength(50)]
		public string EmulatorVersion { get; set; }

		public string OnlineWatchingUrl { get; set; }

		public int Frames { get; set; }
		public int RerecordCount { get; set; }

		// Denormalized name for easy recreation
		public string Title { get; set; }

		public TimeSpan Time
		{
			get
			{
				int seconds = (int)(Frames / SystemFrameRate.FrameRate);
				double fractionalSeconds = (Frames / SystemFrameRate.FrameRate) - seconds;
				int milliseconds = (int)(Math.Round(fractionalSeconds, 2) * 1000);
				var timespan = new TimeSpan(0, 0, 0, seconds, milliseconds);

				return timespan;
			}
		}

		public void GenerateTitle()
		{
			Title =
				$"#{Id} {string.Join(" & ", Authors.Select(sa => sa.Author.UserName))}'s {System.Code} {Game.DisplayName}"
				+ (!string.IsNullOrWhiteSpace(Branch) ? $" \"{Branch}\" " : "")
				+ $" in {Time:g}";
		}
	}
}
