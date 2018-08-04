using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity
{
    /// <summary>
	/// Data storage for an external media post (such as Irc, Discord)
	/// </summary>
	public class MediaPosts : BaseEntity
    {
		public int Id { get; set; }

		[Required]
		public string Title { get; set; }

		[Required]
		public string Link { get; set; }

		[Required]
		public string Body { get; set; }

		[Required]
		public string Group { get; set; }

		[Required]
		public string Type { get; set; }
    }
}
