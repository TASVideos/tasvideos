using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.Entity
{
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

	public interface ISubmissionFilter
	{
		IEnumerable<SubmissionStatus> StatusFilter { get; }
		IEnumerable<int> Years { get; }
		IEnumerable<string> Systems { get; }
		string User { get; }
	}

	public class Submission : BaseEntity, ITimeable
	{
		public int Id { get; set; }

		public int? WikiContentId { get; set; }
		public virtual WikiPage WikiContent { get; set; }

		public int? SubmitterId { get; set; }
		public virtual User Submitter { get; set; }

		public virtual ICollection<SubmissionAuthor> SubmissionAuthors { get; set; } = new HashSet<SubmissionAuthor>();

		public int? IntendedTierId { get; set; }
		public virtual Tier IntendedTier { get; set; }

		public int? JudgeId { get; set; }
		public virtual User Judge { get; set; }

		public virtual User Publisher { get; set; }

		public SubmissionStatus Status { get; set; } = SubmissionStatus.New;
		public virtual ICollection<SubmissionStatusHistory> History { get; set; } = new HashSet<SubmissionStatusHistory>();

		// TODO: we eventually should want to move these to the file server instead
		public byte[] MovieFile { get; set; }

		public string MovieExtension { get; set; }

		public int? GameId { get; set; }
		public virtual Game.Game Game { get; set; }

		public int? RomId { get; set; }
		public virtual GameRom Rom { get; set; }

		// Metadata parsed from movie file
		public int? SystemId { get; set; }
		public virtual GameSystem System { get; set; }

		public int? SystemFrameRateId { get; set; }
		public virtual GameSystemFrameRate SystemFrameRate { get; set; }

		public virtual Publication Publication { get; set; }

		public int Frames { get; set; }
		public int RerecordCount { get; set; }

		// Metadata, user entered
		[StringLength(100)]
		public string EncodeEmbedLink { get; set; }

		[StringLength(100)]
		public string GameVersion { get; set; }

		[StringLength(100)]
		public string GameName { get; set; }

		[StringLength(50)]
		public string Branch { get; set; }

		[StringLength(250)]
		public string RomName { get; set; }
		
		[StringLength(50)]
		public string EmulatorVersion { get; set; }

		public int? MovieStartType { get; set; }

		/// <summary>
		/// Gets or sets a de-normalized column consisting of the submission title for display when linked or in the queue
		/// ex: N64 The Legend of Zelda: Majora's Mask "low%" in 1:59:01
		/// </summary>
		public string Title { get; set; }

		double ITimeable.FrameRate => SystemFrameRate.FrameRate; 

		public void GenerateTitle()
		{
			Title =
				$"#{Id}: {string.Join(" & ", SubmissionAuthors.Select(sa => sa.Author.UserName))}'s {System.Code} {GameName}"
					+ (!string.IsNullOrWhiteSpace(Branch) ? $" \"{Branch}\" " : "")
					+ $" in {this.Time():g}";
		}
	}

	public static class SubmissionExtensions
	{
		public static IQueryable<Submission> FilterBy(this IQueryable<Submission> query, ISubmissionFilter criteria)
		{
			if (!string.IsNullOrWhiteSpace(criteria.User))
			{
				query = query.Where(s => s.SubmissionAuthors.Any(sa => sa.Author.UserName == criteria.User)
					|| s.Submitter.UserName == criteria.User);
			}

			if (criteria.Years.Any())
			{
				query = query.Where(p => criteria.Years.Contains(p.CreateTimeStamp.Year));
			}

			if (criteria.StatusFilter.Any())
			{
				query = query.Where(s => criteria.StatusFilter.Contains(s.Status));
			}

			if (criteria.Systems.Any())
			{
				query = query.Where(s => criteria.Systems.Contains(s.System.Code));
			}

			return query;
		}
	}
}
