using System;
using System.Collections.Generic;

using TASVideos.Data.Entity;

namespace TASVideos.Models
{
	public class UserFileModel
	{
		public long Id { get; set; }
		public UserFileClass Class { get; set; }
		public string Title { get; set; } = "";
		public string? Description { get; set; }
		public DateTime UploadTimestamp { get; set; }
		public string Author { get; set; } = "";
		public int AuthorUserFilesCount { get; set; }
		public int Views { get; set; }
		public int Downloads { get; set; }
		public bool Hidden { get; set; }
		public string? FileName { get; set; }
		public int FileSizeUncompressed { get; set; }
		public int FileSizeCompressed { get; set; }
		public int? GameId { get; set; }
		public string? GameName { get; set; }
		public string? GameSystem { get; set; }
		public string? System { get; set; }

		// Only relevant to Movies
		public TimeSpan Time => TimeSpan.FromSeconds((double)Length);
		public bool IsMovie => Class == UserFileClass.Movie;

		public decimal Length { get; set; }
		public int Frames { get; set; }
		public int Rerecords { get; set; }

		public IEnumerable<UserFileCommentModel> Comments { get; set; } = new List<UserFileCommentModel>();

		public class UserFileCommentModel
		{
			public int Id { get; set; }
			public string Text { get; set; } = "";
			public DateTime CreationTimeStamp { get; set; }
			public int UserId { get; set; }
			public string UserName { get; set; } = "";
		}
	}
}
