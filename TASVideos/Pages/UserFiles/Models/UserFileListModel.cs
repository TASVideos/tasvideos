using System.ComponentModel.DataAnnotations;
using TASVideos.Core;

namespace TASVideos.Pages.UserFiles.Models;

public class UserFileListModel
{
	[TableIgnore]
	public long Id { get; set; }

	public string Title { get; set; } = "";

	[TableIgnore]
	public string FileName { get; set; } = "";

	[Sortable]
	public string Author { get; set; } = "";

	[TableIgnore]
	public int? GameId { get; set; }

	[Sortable]
	[Display(Name = "Game")]
	public string GameName { get; set; } = "";

	[Sortable]
	public int Frames { get; set; }

	[Sortable]
	public int Rerecords { get; set; }

	[Sortable]
	[Display(Name = "Comments")]
	public int CommentCount { get; set; }

	[Sortable]
	[Display(Name = "Uploaded")]
	public DateTime UploadTimestamp { get; set; }
}
