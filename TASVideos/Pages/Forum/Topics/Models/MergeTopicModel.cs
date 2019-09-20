using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Topics.Models
{
	public class MergeTopicModel
	{
		public int ForumId { get; set; }
		public string ForumName { get; set; }
		public string Title { get; set; }

		public string NewTopicTitle { get; set; }

		[Display(Name = "Forum To Merge In to")]
		public int DestinationForumId { get; set; }

		[Required]
		[Display(Name = "Topic To Merge In to")]
		public int DestinationTopicId { get; set; }
	}
}
