using System.ComponentModel.DataAnnotations;

using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications.Models;

public class PublicationEditModel
{
	public string SystemCode { get; set; } = "";

	public string Title { get; set; } = "";

	public string MovieFileName { get; set; } = "";

	[Display(Name = "Additional Authors", Description = "Only authors not registered for TASVideos should be listed here. If multiple authors, separate the names with a comma.")]
	public string? AdditionalAuthors { get; set; }

	[Display(Name = "Author")]
	public IEnumerable<string> Authors { get; set; } = new List<string>();

	[Display(Name = "Publication Class")]
	public string Class { get; set; } = "";
	public string? ClassIconPath { get; set; } = "";
	public string ClassLink { get; set; } = "";

	[Display(Name = "Obsoleted By")]
	public int? ObsoletedBy { get; set; }

	[StringLength(50)]
	[Display(Name = "Emulator Version")]
	public string? EmulatorVersion { get; set; }

	public string? Branch { get; set; }

	[Display(Name = "Selected Flags")]
	public IEnumerable<int> SelectedFlags { get; set; } = new List<int>();

	[Display(Name = "Selected Tags")]
	public IEnumerable<int> SelectedTags { get; set; } = new List<int>();

	[Display(Name = "Revision Message")]
	public string? RevisionMessage { get; set; }

	[Display(Name = "Minor Edit")]
	public bool MinorEdit { get; set; }

	public string Markup { get; set; } = "";

	public IEnumerable<PublicationUrlDisplayModel> Urls { get; set; } = new List<PublicationUrlDisplayModel>();
}

public class PublicationFileDisplayModel
{
	public int Id { get; set; }
	public string Path { get; set; } = "";
	public FileType Type { get; set; }
	public string? Description { get; set; }
}

public class PublicationUrlDisplayModel
{
	public int Id { get; set; }
	public string Url { get; set; } = "";
	public PublicationUrlType Type { get; set; }
	public string? DisplayName { get; set; }
}
