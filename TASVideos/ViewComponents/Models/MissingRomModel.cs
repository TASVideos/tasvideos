using System.Collections.Generic;

namespace TASVideos.ViewComponents.Models
{
	public class MissingRomModel
	{
		public ICollection<Entry> Publications { get; init; } = new List<Entry>();
		public ICollection<Entry> Submissions { get; init; } = new List<Entry>();
		public record Entry(int Id, string Title);
	}
}
