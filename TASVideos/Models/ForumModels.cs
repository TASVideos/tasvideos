using System.Collections.Generic;

using TASVideos.Data.Entity.Forum;

namespace TASVideos.Models
{
	public class ForumIndexModel
	{
		public IEnumerable<ForumCategory> Categories { get; set; } = new List<ForumCategory>();
	}
}
