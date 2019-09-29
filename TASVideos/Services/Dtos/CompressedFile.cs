using TASVideos.Data.Entity;

namespace TASVideos.Services
{
	public class CompressedFile
	{
		public string FileName { get; set; }
		public int OriginalSize { get; set; }
		public int CompressedSize { get; set; }
		public Compression Type { get; set; }

		public byte[] Data { get; set; }
	}
}
