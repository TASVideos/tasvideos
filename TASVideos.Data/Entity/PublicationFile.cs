using System.ComponentModel.DataAnnotations;

namespace TASVideos.Data.Entity
{
	public enum FileType
	{
		Screenshot, MovieFile, Torrent
	}

	public class PublicationFile : BaseEntity
	{
		public int Id { get; set; }

		public int PublicationId { get; set; }
		public virtual Publication Publication { get; set; }

		[StringLength(250)]
		public string Path { get; set; }
		public FileType Type { get; set; }

		[StringLength(250)]
		public string Description { get; set; }
	}
}
