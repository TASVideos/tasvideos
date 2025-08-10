using System.IO.Compression;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TASVideos.Data.Entity;
using TASVideos.Extensions;

namespace TASVideos.RazorPages.Tests.Extensions;

[TestClass]
public class FormFileExtensionTests
{
	[TestMethod]
	public void IsZip_NullReturnsFalse()
	{
		var actual = ((IFormFile?)null).IsZip();
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public void IsZip_FileExtensionIsNotZip_ReturnsFalse()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns("test.notzip");
		formFile.ContentType.Returns("application/zip");

		var actual = ((IFormFile?)null).IsZip();
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public void IsZip_InvalidContentType_ReturnsFalse()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns("test.zip");
		formFile.ContentType.Returns("application/notzip");

		var actual = ((IFormFile?)null).IsZip();
		Assert.IsFalse(actual);
	}

	[TestMethod]
	[DataRow("application/x-zip-compressed")]
	[DataRow("application/zip")]
	public void IsZip_HasCorrectExtensionAndContentType_ReturnsTrue(string contentType)
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns("test.zip");
		formFile.ContentType.Returns(contentType);

		var actual = formFile.IsZip();
		Assert.IsTrue(actual);
	}

	[TestMethod]
	public void IsCompressed_NullReturnsFalse()
	{
		var actual = ((IFormFile?)null).IsCompressed();
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public void IsCompressed_InvalidExtensionAndContentType_ReturnsFalse()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns("test.notzip");
		formFile.ContentType.Returns("application/notzip");

		var actual = formFile.IsCompressed();
		Assert.IsFalse(actual);
	}

	[TestMethod]
	[DataRow(".zip")]
	[DataRow(".gz")]
	[DataRow(".bz2")]
	[DataRow(".lzma")]
	[DataRow(".xz")]
	public void IsCompressed_ValidExtension_ReturnsTrue(string extension)
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns($"test{extension}");
		formFile.ContentType.Returns("application/notzip");

		var actual = formFile.IsCompressed();
		Assert.IsTrue(actual);
	}

	[TestMethod]
	[DataRow("application/x-zip-compressed")]
	[DataRow("application/zip")]
	[DataRow("applicationx-gzip")]
	public void IsCompressed_ValidContentType_ReturnsTrue(string contentType)
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns("test.notzip");
		formFile.ContentType.Returns(contentType);

		var actual = formFile.IsCompressed();
		Assert.IsTrue(actual);
	}

	[TestMethod]
	public void AddModelErrorIfOverSizeLimit_OverLimitAndNoPermission_AddsModelError()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.Length.Returns(SiteGlobalConstants.MaximumMovieSize);
		var modelState = Substitute.For<ModelStateDictionary>();
		var user = new ClaimsPrincipal();

		formFile.AddModelErrorIfOverSizeLimit(modelState, user);
		Assert.AreEqual(1, modelState.Count);
	}

	[TestMethod]
	public void AddModelErrorIfOverSizeLimit_OverLimitAndPermission_Allowed()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.Length.Returns(SiteGlobalConstants.MaximumMovieSize);
		var modelState = Substitute.For<ModelStateDictionary>();
		var user = RazorTestHelpers.CreateClaimsPrincipalWithPermissions([PermissionTo.OverrideSubmissionConstraints]);

		formFile.AddModelErrorIfOverSizeLimit(modelState, user);
		Assert.AreEqual(0, modelState.Count);
	}

	[TestMethod]
	public void AddModelErrorIfOverSizeLimit_UnderLimitAndNoPermission_Allowed()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.Length.Returns(1);
		var modelState = Substitute.For<ModelStateDictionary>();
		var user = new ClaimsPrincipal();

		formFile.AddModelErrorIfOverSizeLimit(modelState, user);
		Assert.AreEqual(0, modelState.Count);
	}

	[TestMethod]
	public void IsValidImage_NullReturnsFalse()
	{
		var actual = ((IFormFile?)null).IsValidImage();
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public void IsValidImage_InvalidContentType_ReturnsFalse()
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.ContentType.Returns("notimage");

		var actual = formFile.IsValidImage();
		Assert.IsFalse(actual);
	}

	[TestMethod]
	[DataRow("image/png")]
	[DataRow("image/jpeg")]
	public void IsValidImage_ValidContentType_ReturnsTrue(string contentType)
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.ContentType.Returns(contentType);

		var actual = formFile.IsValidImage();
		Assert.IsTrue(actual);
	}

	[TestMethod]
	public void FileExtension_NullReturnsEmptyString()
	{
		var actual = ((IFormFile?)null).FileExtension();
		Assert.AreEqual("", actual);
	}

	[TestMethod]
	[DataRow("", "")]
	[DataRow("noextension", "")]
	[DataRow("file.zip", ".zip")]
	[DataRow(".zip", ".zip")]
	public void FileExtension(string fileName, string expected)
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns(fileName);

		var actual = formFile.FileExtension();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task ToBytes_NullReturnsEmptyByteArray()
	{
		var actual = await ((IFormFile?)null).ToBytes();
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Length);
	}

	[TestMethod]
	public async Task ToBytes_ReturnsByteArray()
	{
		byte[] bytes = [.. Enumerable.Repeat<byte>(0xFF, 10)];
		var ms = new MemoryStream(bytes);
		var formFile = new FormFile(ms, 0, bytes.Length, "Data", "test.bk2");

		var actual = await formFile.ToBytes();
		Assert.IsNotNull(actual);
		Assert.AreEqual(bytes.Length, actual.Length);
	}

	[TestMethod]
	public async Task DecompressOrTakeRaw_NullReturnsEmptyStream()
	{
		var actual = await ((IFormFile?)null).ToBytes();
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Length);
	}

	[TestMethod]
	public async Task DecompressOrTakeRaw_NullFile_ReturnsEmptyStream()
	{
		var actual = await ((IFormFile?)null).DecompressOrTakeRaw();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Length);
	}

	[TestMethod]
	public async Task DecompressOrTakeRaw_NotGzip_ReturnsRaw()
	{
		const int length = 10;
		byte[] bytes = [.. Enumerable.Repeat<byte>(0xFF, length)];
		var ms = new MemoryStream(bytes);
		var formFile = new FormFile(ms, 0, bytes.Length, "Data", "test.bk2");

		var actual = await formFile.DecompressOrTakeRaw();

		Assert.IsNotNull(actual);
		Assert.AreEqual(length, actual.Length);
	}

	[TestMethod]
	public async Task DecompressOrTakeRaw_ValidGzip_ReturnsGzip()
	{
		(byte[] bytes, int uncompressedLength) = GZippedBytes();
		var ms = new MemoryStream(bytes);
		var formFile = new FormFile(ms, 0, bytes.Length, "Data", "test.bk2");

		var actual = await formFile.DecompressOrTakeRaw();
		Assert.IsNotNull(actual);
		Assert.AreEqual(uncompressedLength, actual.Length);
	}

	private static (byte[] GzippedBytes, int UncompressedLength) GZippedBytes()
	{
		byte[] data = "Hello World"u8.ToArray();
		using var ms = new MemoryStream();
		using var zipStream = new GZipStream(ms, CompressionMode.Compress);
		zipStream.Write(data, 0, data.Length);
		zipStream.Close();
		return (ms.ToArray(), data.Length);
	}
}
