using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity;

[IncludeInAutoHistory]
[Index(nameof(PageNameFrom), IsUnique = true)]
public class WikiRedirect : BaseEntity
{
	public int Id { get; set; }
	public string PageNameFrom { get; set; } = "";
	public string PageNameTo { get; set; } = "";
}
