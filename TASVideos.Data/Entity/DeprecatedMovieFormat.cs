using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data.Entity;

[ExcludeFromHistory]
public class DeprecatedMovieFormat : BaseEntity
{
	public int Id { get; set; }

	[Required]
	public string FileExtension { get; set; } = "";

	public bool Deprecated { get; set; } = true;
}
