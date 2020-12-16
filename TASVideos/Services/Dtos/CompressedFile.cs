using TASVideos.Data.Entity;

namespace TASVideos.Services
{
	public record CompressedFile(
		int OriginalSize,
		int CompressedSize,
		Compression Type,
		byte[] Data);
}
