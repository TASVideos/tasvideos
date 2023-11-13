using TASVideos.Data.Entity;

namespace TASVideos.Models;

public class UserFileModel
{
	public long Id { get; set; }
	public UserFileClass Class { get; set; }
	public string Title { get; set; } = "";
	public string? Description { get; set; }
	public DateTime UploadTimestamp { get; set; }
	public string Author { get; set; } = "";
	public int AuthorUserFilesCount { get; set; }
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
	public TimeSpan Time => TimeSpan.FromSeconds(Math.Round((double)Length, 2, MidpointRounding.AwayFromZero));
	public bool IsMovie => Class == UserFileClass.Movie;

	public decimal Length { get; set; }
	public int Frames { get; set; }
	public int Rerecords { get; set; }

	public IEnumerable<UserFileCommentModel> Comments { get; set; } = new List<UserFileCommentModel>();
	public bool HideComments { get; set; }
	public Compression CompressionType { get; set; }
	public byte[] Content { get; set; } = Array.Empty<byte>();
	public string ContentPreview { get; set; } = "";

	public string Extension => (FileName ?? "").ToLower().Split('.').Last();
	public string? Annotations { get; set; }

	public class UserFileCommentModel
	{
		public int Id { get; init; }
		public string Text { get; init; } = "";
		public DateTime CreationTimeStamp { get; init; }
		public int UserId { get; init; }
		public string UserName { get; init; } = "";
	}
}
