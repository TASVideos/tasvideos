using TASVideos.Data.Entity;

namespace TASVideos.Models;

public class UserFileModel
{
	public long Id { get; init; }
	public UserFileClass Class { get; init; }
	public string Title { get; init; } = "";
	public string? Description { get; init; }
	public DateTime UploadTimestamp { get; init; }
	public string Author { get; init; } = "";
	public int AuthorUserFilesCount { get; init; }
	public int Downloads { get; init; }
	public bool Hidden { get; init; }
	public string? FileName { get; init; }
	public int FileSizeUncompressed { get; init; }
	public int FileSizeCompressed { get; init; }
	public int? GameId { get; init; }
	public string? GameName { get; init; }
	public string? GameSystem { get; init; }
	public string? System { get; init; }

	// Only relevant to Movies
	public TimeSpan Time => TimeSpan.FromSeconds(Math.Round((double)Length, 2, MidpointRounding.AwayFromZero));
	public bool IsMovie => Class == UserFileClass.Movie;

	public decimal Length { get; init; }
	public int Frames { get; init; }
	public int Rerecords { get; init; }

	public List<UserFileCommentModel> Comments { get; init; } = [];
	public bool HideComments { get; init; }
	public Compression CompressionType { get; set; }
	public byte[] Content { get; set; } = [];
	public string ContentPreview { get; set; } = "";

	public string Extension => (FileName ?? "").ToLower().Split('.').Last();
	public string? Annotations { get; init; }

	public record UserFileCommentModel(int Id, string Text, DateTime CreationTimeStamp, int UserId, string UserName);
}
