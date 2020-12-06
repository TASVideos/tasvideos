using System;
using TASVideos.Data.Entity;

namespace TASVideos.Services
{
	public class CompressedFile
	{
		public string FileName { get; init; } = "";
		public int OriginalSize { get; init; }
		public int CompressedSize { get; set; }
		public Compression Type { get; set; }

		public byte[] Data { get; set; } = Array.Empty<byte>();
	}
}
