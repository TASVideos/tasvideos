using System.Collections.Generic;

namespace TASVideos.Models
{
	public class ForumIndexModel
	{
		public IEnumerable<ForumCategoryModel> Categories { get; set; } = new List<ForumCategoryModel>();
	}

	public class ForumCategoryModel
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public int Ordinal { get; set; }
	}
}
