using TASVideos.Pages;

namespace TASVideos.RazorPages.Tests.Pages;

[TestClass]
public class PageExtensionTests
{
	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow("-")]
	public void ToTokens_Null_ReturnsEmptyList(string source)
	{
		var actual = source.ToTokens();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	[DataRow("token1-token2", "token1", "token2")]
	[DataRow("Token1-Token2", "token1", "token2")]
	[DataRow("token1 - token2", "token1", "token2")]
	[DataRow(" token1 - token2 ", "token1", "token2")]
	[DataRow("token1-token2-", "token1", "token2")]
	[DataRow("-token1-token2", "token1", "token2")]
	[DataRow(" - - - token1", "token1")]
	public void ToTokens(string source, params string[] expected)
	{
		var actual = source.ToTokens();

		Assert.IsNotNull(actual);
		Assert.AreEqual(expected.Length, actual.Count);
		Assert.IsTrue(expected.OrderBy(e => e).SequenceEqual(actual.OrderBy(a => a)));
	}

	[TestMethod]
	[DataRow(new[] { "1M", "2M" }, 'M', new[] { 1, 2 })]
	[DataRow(new[] { "1M", "2M" }, 'm', new[] { 1, 2 })]
	[DataRow(new[] { "1G", "2g" }, 'g', new[] { 1, 2 })]
	[DataRow(new[] { "1M", "2M", "NotANumberM" }, 'm', new[] { 1, 2 })]
	public void ToIdList(string[] source, char suffix, int[] expected)
	{
		var result = source.ToIdList(suffix);
		Assert.IsNotNull(result);
		var actual = result.ToList();
		Assert.AreEqual(expected.Length, actual.Count);
		Assert.IsTrue(expected.OrderBy(e => e).SequenceEqual(actual.OrderBy(a => a)));
	}

	[TestMethod]
	[DataRow(new[] { "Group1", "Group2" }, "Group", new[] { 1, 2 })]
	public void ToIdListPrefix(string[] source, string prefix, int[] expected)
	{
		var result = source.ToIdListPrefix(prefix);
		Assert.IsNotNull(result);
		var actual = result.ToList();
		Assert.AreEqual(expected.Length, actual.Count);
		Assert.IsTrue(expected.OrderBy(e => e).SequenceEqual(actual.OrderBy(a => a)));
	}
}
