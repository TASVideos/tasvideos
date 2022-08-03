using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class PublicationClass
{
	public int Id { get; set; }

	[Required]
	[StringLength(20)]
	public string Name { get; set; } = "";
	public double Weight { get; set; }

	[StringLength(100)]
	public string? IconPath { get; set; }

	[Required]
	[StringLength(100)]
	public string Link { get; set; } = "";
}
