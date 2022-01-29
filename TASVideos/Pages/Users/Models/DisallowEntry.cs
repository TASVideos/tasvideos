using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Users.Models;

public class DisallowEntry
{
	public int Id { get; set; }

	[Required]
	public string? RegexPattern { get; set; }
}
