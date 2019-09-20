using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Topics.Models
{
	public class MergeTopicModel
	{
		public string NewTopicTitle { get; set; }

		[Required]
		public int TopicToMergeId { get; set; }

		public string TopicToMergeTitle { get; set; }

		[Required]
		public int DestinationTopicId { get; set; }
	}
}
