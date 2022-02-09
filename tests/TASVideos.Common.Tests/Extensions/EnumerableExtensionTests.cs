using TASVideos.Extensions;

namespace TASVideos.Common.Tests.Extensions;

[TestClass]
public class EnumerableExtensionTests
{
	[TestMethod]
	[DataRow(new int[0], new int[0])]
	[DataRow(new[] { 1, 2 }, new[] { 1 })]
	[DataRow(new[] { 1, 2, 3 }, new[] { 1, 2 })]
	[DataRow(new[] { 1, 2, 3, 4 }, new[] { 1, 2 })]
	public void FirstHalf_Tests(int[] list, int[] expected)
	{
		var actual = list.FirstHalf();
		Assert.IsTrue(expected.SequenceEqual(actual));
	}

	[TestMethod]
	[DataRow(new int[0], new int[0])]
	[DataRow(new[] { 1, 2 }, new[] { 2 })]
	[DataRow(new[] { 1, 2, 3 }, new[] { 3 })]
	[DataRow(new[] { 1, 2, 3, 4 }, new[] { 3, 4 })]
	public void SecondHalf_Tests(int[] list, int[] expected)
	{
		var actual = list.SecondHalf();
		Assert.IsTrue(expected.SequenceEqual(actual));
	}

	[TestMethod]
	public void AtRandom_Basic_Test()
	{
		var collection = new[] { 1, 2, 3, 4, 5 };
		var actual = collection.AtRandom();
		Assert.IsTrue(collection.Contains(actual));
	}
}
