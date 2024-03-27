using TASVideos.Core;

namespace TASVideos.Pages.UserFiles.Models;

public class UserFileListModel
{
	[TableIgnore]
	public long Id { get; init; }

	public string Title { get; init; } = "";

	[TableIgnore]
	public string FileName { get; init; } = "";

	[Sortable]
	public string Author { get; init; } = "";

	[TableIgnore]
	public int? GameId { get; init; }

	[Sortable]
	[Display(Name = "Game")]
	public string GameName { get; init; } = "";

	[Sortable]
	public int Frames { get; init; }

	[Sortable]
	public int Rerecords { get; init; }

	[Sortable]
	[Display(Name = "Comments")]
	public int CommentCount { get; init; }

	[Sortable]
	[Display(Name = "Uploaded")]
	public DateTime UploadTimestamp { get; init; }
}
