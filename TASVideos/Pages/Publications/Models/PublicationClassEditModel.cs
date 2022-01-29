using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Publications.Models;

public class PublicationClassEditModel
{
	public string Title { get; set; } = "";

	[Display(Name = "PublicationClass")]
	public int ClassId { get; set; }
}
