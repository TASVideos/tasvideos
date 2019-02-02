using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Topics.Models
{
	public class MoveTopicModel
	{
		[Display(Name = "New Forum")]
		public int ForumId { get; set; }

		[Display(Name = "Topic")]
		public string TopicTitle { get; set; }

		[Display(Name = "Current Forum")]
		public string ForumName { get; set; }
	}
}
