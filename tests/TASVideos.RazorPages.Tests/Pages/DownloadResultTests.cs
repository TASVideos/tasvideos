using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services;
using TASVideos.Pages;

namespace TASVideos.RazorPages.Tests.Pages;

[TestClass]
public class DownloadResultTests : BasePageModelTests
{
	[TestMethod]
	public async Task ExecuteResultAsync_NullFile_Returns404()
	{
		var downloadResult = new DownloadResult(null);
		var httpContext = new DefaultHttpContext();
		var actionContext = new ActionContext
		{
			HttpContext = httpContext
		};

		await downloadResult.ExecuteResultAsync(actionContext);

		Assert.AreEqual(404, httpContext.Response.StatusCode);
	}

	[TestMethod]
	public async Task ExecuteResultAsync_ValidFile_Returns200()
	{
		var file = new DownloadableFile("test.bk2", [1, 2, 3], Compression.None);
		var downloadResult = new DownloadResult(file);
		var actionContext = TestActionContext();

		await downloadResult.ExecuteResultAsync(actionContext);

		Assert.AreEqual(200, actionContext.HttpContext.Response.StatusCode);
	}

	[TestMethod]
	public async Task ExecuteResultAsync_ValidFile_SetsContentLength()
	{
		byte[] content = [1, 2, 3, 4, 5];
		var file = new DownloadableFile("test.bk2", content, Compression.None);
		var downloadResult = new DownloadResult(file);
		var actionContext = TestActionContext();

		await downloadResult.ExecuteResultAsync(actionContext);

		Assert.AreEqual(content.Length.ToString(), actionContext.HttpContext.Response.Headers["Content-Length"].ToString());
	}

	[TestMethod]
	public async Task ExecuteResultAsync_ValidFile_SetsContentType()
	{
		var file = new DownloadableFile("test.bk2", [1, 2, 3], Compression.None);
		var downloadResult = new DownloadResult(file);
		var actionContext = TestActionContext();

		await downloadResult.ExecuteResultAsync(actionContext);

		Assert.AreEqual("application/octet-stream", actionContext.HttpContext.Response.Headers["Content-Type"].ToString());
	}

	[TestMethod]
	public async Task ExecuteResultAsync_ValidFile_SetsContentDisposition()
	{
		var file = new DownloadableFile("test.bk2", [1, 2, 3], Compression.None);
		var downloadResult = new DownloadResult(file);
		var actionContext = TestActionContext();

		await downloadResult.ExecuteResultAsync(actionContext);

		var contentDisposition = actionContext.HttpContext.Response.Headers.ContentDisposition.ToString();
		Assert.IsTrue(contentDisposition.Contains("attachment"));
		Assert.IsTrue(contentDisposition.Contains("test.bk2"));
	}

	[TestMethod]
	public async Task ExecuteResultAsync_GzipCompression_SetsContentEncoding()
	{
		var file = new DownloadableFile("test.bk2", [1, 2, 3], Compression.Gzip);
		var downloadResult = new DownloadResult(file);
		var actionContext = TestActionContext();

		await downloadResult.ExecuteResultAsync(actionContext);

		Assert.AreEqual("gzip", actionContext.HttpContext.Response.Headers["Content-Encoding"].ToString());
	}

	[TestMethod]
	public async Task ExecuteResultAsync_ZipCompression_SetsContentEncoding()
	{
		var file = new DownloadableFile("test.bk2", [1, 2, 3], Compression.Zip);
		var downloadResult = new DownloadResult(file);
		var actionContext = TestActionContext();

		await downloadResult.ExecuteResultAsync(actionContext);

		Assert.AreEqual("zip", actionContext.HttpContext.Response.Headers["Content-Encoding"].ToString());
	}

	[TestMethod]
	public async Task ExecuteResultAsync_NoCompression_DoesNotSetContentEncoding()
	{
		var file = new DownloadableFile("test.bk2", [1, 2, 3], Compression.None);
		var downloadResult = new DownloadResult(file);
		var actionContext = TestActionContext();

		await downloadResult.ExecuteResultAsync(actionContext);

		Assert.IsFalse(actionContext.HttpContext.Response.Headers.ContainsKey("Content-Encoding"));
	}

	[TestMethod]
	public async Task ExecuteResultAsync_ValidFile_WritesContentToResponseBody()
	{
		byte[] content = [1, 2, 3, 4, 5];
		var file = new DownloadableFile("test.bk2", content, Compression.None);
		var downloadResult = new DownloadResult(file);
		var actionContext = TestActionContext();

		await downloadResult.ExecuteResultAsync(actionContext);

		var responseBody = (MemoryStream)actionContext.HttpContext.Response.Body;
		responseBody.Position = 0;
		var writtenContent = new byte[responseBody.Length];
		await responseBody.ReadExactlyAsync(writtenContent, 0, (int)responseBody.Length);

		CollectionAssert.AreEqual(content, writtenContent);
	}
}
