using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

using ZstdSharp;

namespace TASVideos.Services;

/// <summary>Implements stream compression via ZstdSharp, for use by the <see cref="ResponseCompressionMiddleware"/></summary>
/// <remarks>To the extent that this shim is eligible for copyright, it's taken from this GPLv2-licensed source: <see href="https://github.com/rgueldenpfennig/Squidlr/commit/07ab9aaec37a0b24bc9ff3af84a3d61b150d59f9"/></remarks>
public sealed class ZstdCompressionProvider(IOptions<ZstdCompressionProvider.ZstdOptions> options) : ICompressionProvider
{
	public sealed class ZstdOptions : IOptions<ZstdOptions>
	{
		public int Level { get; set; } = 1;

		public ZstdOptions Value
			=> this;
	}

	public string EncodingName
		=> "zstd";

	public bool SupportsFlush
		=> true;

	public Stream CreateStream(Stream outputStream)
		=> new CompressionStream(outputStream, options.Value.Level, leaveOpen: true);
}
