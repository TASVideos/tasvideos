using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Extensions;

namespace TASVideos.Test.Common.Extensions
{
	[TestClass]
	public class EnumerableExtensionTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void FirstHalf_Null_Throws()
		{
			((IEnumerable<int>)null).FirstHalf();
		}

		[TestMethod]
		[DataRow(new int[0], new int[0])]
		[DataRow(new[] { 1, 2 }, new[] { 1 })]
		[DataRow(new[] { 1, 2, 3 }, new[] { 1, 2 })]
		[DataRow(new[] { 1, 2, 3, 4 }, new[] { 1, 2 })]
		public void FirstHalf_Tests(int[] list, int[] expected)
		{
			var actual = list.FirstHalf();
			Assert.IsNotNull(actual);
			Assert.IsTrue(expected.SequenceEqual(actual));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SecondHalf_Null_Throws()
		{
			((IEnumerable<int>)null).SecondHalf();
		}

		[TestMethod]
		[DataRow(new int[0], new int[0])]
		[DataRow(new[] { 1, 2 }, new[] { 2 })]
		[DataRow(new[] { 1, 2, 3 }, new[] { 3 })]
		[DataRow(new[] { 1, 2, 3, 4 }, new[] { 3, 4 })]
		public void SecondHalf_Tests(int[] list, int[] expected)
		{
			var actual = list.SecondHalf();
			Assert.IsNotNull(actual);
			Assert.IsTrue(expected.SequenceEqual(actual));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void AtRandom_Null_Throws()
		{
			((ICollection<int>)null).AtRandom();
		}

		[TestMethod]
		public void AtRandom_Basic_Test()
		{
			var collection = new[] { 1, 2, 3, 4, 5 };
			var actual = collection.AtRandom();
			Assert.IsNotNull(actual);
			Assert.IsTrue(collection.Contains(actual));
		}
	}
}
