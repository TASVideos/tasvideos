using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.Publications.Models;

public class PublicationEditModel
{
	public string SystemCode { get; set; } = "";

	public string Title { get; set; } = "";

	public string MovieFileName { get; set; } = "";

	[Display(Name = "External Authors", Description = "Only authors not registered for TASVideos should be listed here. If multiple authors, separate the names with a comma.")]
	public string? AdditionalAuthors { get; set; }

	[Display(Name = "Author(s)")]
	public List<string> Authors { get; set; } = [];

	[Display(Name = "Publication Class")]
	public string Class { get; set; } = "";
	public string? ClassIconPath { get; set; } = "";
	public string ClassLink { get; set; } = "";

	[Display(Name = "Obsoleted By")]
	public int? ObsoletedBy { get; set; }

	public string? ObsoletedByTitle { get; set; }

	[StringLength(50)]
	[Display(Name = "Emulator Version")]
	public string? EmulatorVersion { get; set; }

	[Display(Name = "Selected Flags")]
	public List<int> SelectedFlags { get; set; } = [];

	[Display(Name = "Selected Tags")]
	public List<int> SelectedTags { get; set; } = [];

	[Display(Name = "Revision Message")]
	public string? RevisionMessage { get; set; }

	[Display(Name = "Minor Edit")]
	public bool MinorEdit { get; set; }

	[DoNotTrim]
	public string Markup { get; set; } = "";

	public List<PublicationUrlDisplayModel> Urls { get; set; } = [];
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
