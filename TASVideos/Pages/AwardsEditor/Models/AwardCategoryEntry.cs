using TASVideos.Data.Entity.Awards;

namespace TASVideos.Pages.AwardsEditor.Models;

public class AwardCategoryEntry
{
	public int Id { get; set; }
	public AwardType Type { get; set; }

	public string ShortName { get; set; } = "";

	public string Description { get; set; } = "";

	public bool InUse { get; set; }
}
