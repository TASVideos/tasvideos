namespace TASVideos.Data.Entity
{
	public enum Compression
	{
		None,
		Gzip
	}

	public class DatabaseFile
	{
		public int Id { get; set; }

		public string Filename { get; set; }

		public Compression Compression { get; set; }

		public int CompressedSize { get; set; }

		public int OriginalSize { get; set; }

		public byte[] Data { get; set; }
	}
}
