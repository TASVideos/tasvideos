using Microsoft.Net.Http.Headers;

namespace TASVideos.Pages;

internal class DownloadResult(DownloadableFile? file) : IActionResult
{
	public Task ExecuteResultAsync(ActionContext context)
	{
		var res = context.HttpContext.Response;
		if (file is null)
		{
			res.StatusCode = 404;
			return res.Body.WriteAsync([], 0, 0);
		}

		res.Headers.Append("Content-Length", file.Content.Length.ToString());
		switch (file.CompressionType)
		{
			case Compression.Gzip:
				res.Headers.Append("Content-Encoding", "gzip");
				break;
			case Compression.Zip:
				res.Headers.Append("Content-Encoding", "zip");
				break;
		}

		res.Headers.Append("Content-Type", "application/octet-stream");
		var contentDisposition = new ContentDispositionHeaderValue("attachment");
		contentDisposition.SetHttpFileName(file.FileName);
		res.Headers.ContentDisposition = contentDisposition.ToString();
		res.StatusCode = 200;
		return res.Body.WriteAsync(file.Content, 0, file.Content.Length);
	}
}
