using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Pages;

namespace TASVideos.Test.Pages
{
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
	}
}
