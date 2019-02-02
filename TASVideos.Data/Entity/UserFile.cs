using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity
{
	public enum UserFileClass
	{
		[Display(Name = "Movie")]
		Movie,

		[Display(Name = "Support file")]
		Support
	}

	public class UserFile
	{
		[Key]
		public long Id { get; set; }

		public int AuthorId { get; set; }
		public virtual User Author { get; set; }

		[StringLength(255)]
		public string FileName { get; set; }

		public byte[] Content { get; set; }

		public UserFileClass Class { get; set; }

		[StringLength(16)]
		public string Type { get; set; }

		public DateTime UploadTimestamp { get; set; }

		public decimal Length { get; set; }

		public int Frames { get; set; }

		public int Rerecords { get; set; }

		[StringLength(255)]
		public string Title { get; set; }

		public string Description { get; set; }

		public int LogicalLength { get; set; }

		public int PhysicalLength { get; set; }

		public int? GameId { get; set; }
		public virtual Game.Game Game { get; set; }

		public int? SystemId { get; set; }
		public virtual Game.GameSystem System { get; set; }

		public bool Hidden { get; set; }

		public string Warnings { get; set; }

		public int Views { get; set; }

		public int Downloads { get; set; }

		public virtual ICollection<UserFileComment> Comments { get; set; }
	}

	public static class UserFileExtensions
	{
		public static IQueryable<UserFile> ThatArePublic(this IQueryable<UserFile> query)
		{
			return query.Where(q => !q.Hidden);
		}

		public static IQueryable<UserFile> ByRecentlyUploaded(this IQueryable<UserFile> query)
		{
			return query.OrderByDescending(q => q.UploadTimestamp);
		}
	}
}
