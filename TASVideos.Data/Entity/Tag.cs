using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity;

[IncludeInAutoHistory]
public class Tag
{
	public int Id { get; set; }

	public string Code { get; set; } = "";

	public string DisplayName { get; set; } = "";

	public ICollection<PublicationTag> PublicationTags { get; init; } = [];
}
