namespace TASVideos.Data.Entity
{
	public enum FileType
	{
		Screenshot, MovieFile
	}

	public class PublicationFile : BaseEntity
	{
		public int Id { get; set; }
		public virtual Publication Publication { get; set; }

		public string Path { get; set; }
		public FileType Type { get; set; }
	}
}
