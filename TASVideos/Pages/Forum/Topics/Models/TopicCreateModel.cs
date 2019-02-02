using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Topics.Models
{
	public class TopicCreateModel
	{
		public string ForumName { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 5)]
		public string Title { get; set; }

		[Required]
		[StringLength(1000, MinimumLength = 5)]
		public string Post { get; set; }

		public ForumTopicType Type { get; set; } = ForumTopicType.Regular;
	}
}
