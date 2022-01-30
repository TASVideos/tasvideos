using TASVideos.Core.Services;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class ListDiffTests
{
	[TestMethod]
	[DataRow(new string[0], new string[0], new string[0], new string[0])]
	[DataRow(new[] { "a" }, new[] { "b" }, new[] { "b" }, new[] { "a" })]
	[DataRow(new[] { "a", "b" }, new[] { "c", "d" }, new[] { "c", "d" }, new[] { "a", "b" })]
	[DataRow(new string[0], new[] { "a", "b" }, new[] { "a", "b" }, new string[0])]
	[DataRow(new[] { "a", "b" }, new string[0], new string[0], new[] { "a", "b" })]
	[DataRow(new[] { "a", "b" }, new[] { "a", "c" }, new[] { "c" }, new[] { "b" })]
	public void BasicTests(
		string[] currentItems,
		string[] newItems,
		string[] expectedAdded,
		string[] expectedRemoved)
	{
		var actual = new ListDiff(currentItems, newItems);

		Assert.IsTrue(actual.Added.SequenceEqual(expectedAdded));
		Assert.IsTrue(actual.Removed.SequenceEqual(expectedRemoved));
	}
}
