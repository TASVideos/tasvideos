using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity
{
	public enum PublicationUrlType { Streaming, Mirror }

	public class PublicationUrl : BaseEntity
	{
		public int Id { get; set; }
		public int PublicationId { get; set; }
		public virtual Publication? Publication { get; set; }

		[Required]
		[StringLength(500)]
		public string? Url { get; set; }

		public PublicationUrlType Type { get; set; } = PublicationUrlType.Streaming;
	}
}
