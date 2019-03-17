using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Data;

// TODO: fix project level settings
// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Data.Paging
{
	[TestClass]
	public class FieldSelectableTests
	{
		private const string TestString = "string";
		private const int TestInt = 1;
		private const bool TestBool = true;

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow("\r \n \t")]
		[DataRow(",,,")]
		public void FieldSelectable_Single_EmptySelect_ReturnsObj(string fields)
		{
			var testClass = new TestClass();
			var actual = testClass.FieldSelect(fields);
			Assert.IsNotNull(actual);
			var dic = (IDictionary<string, object>)actual;
			Assert.AreEqual(3, dic.Count);
			Assert.AreEqual(dic[nameof(TestClass.String)], TestString);
			Assert.AreEqual(dic[nameof(TestClass.Int)], TestInt);
			Assert.AreEqual(dic[nameof(TestClass.Bool)], TestBool);
		}

		[TestMethod]
		public void FieldSelectable_Single_SingleSelect_ReturnsSingle()
		{
			var testClass = new TestClass();
			var actual = testClass.FieldSelect(nameof(TestClass.String));
			Assert.IsNotNull(actual);
			var dic = (IDictionary<string, object>)actual;
			Assert.AreEqual(1, dic.Count);
			Assert.AreEqual(dic[nameof(TestClass.String)], TestString);

			Assert.IsFalse(dic.ContainsKey(nameof(TestClass.Int)));
			Assert.IsFalse(dic.ContainsKey(nameof(TestClass.Bool)));
		}

		[TestMethod]
		public void FieldSelectable_Single_DoubleSelect_ReturnsDouble()
		{
			string fields = $"{nameof(TestClass.String)},{nameof(TestClass.Int)}";
			var testClass = new TestClass();
			var actual = testClass.FieldSelect(fields);
			Assert.IsNotNull(actual);
			var dic = (IDictionary<string, object>)actual;
			Assert.AreEqual(2, dic.Count);
			Assert.AreEqual(dic[nameof(TestClass.String)], TestString);
			Assert.AreEqual(dic[nameof(TestClass.Int)], TestInt);

			Assert.IsFalse(dic.ContainsKey(nameof(TestClass.Bool)));
		}

		[TestMethod]
		public void FieldSelectable_Multi_Distinct()
		{
			var fields = new FieldSelectable($"{nameof(TestClass.String)},{nameof(TestClass.Bool)}");
			var testList = new[]
			{
				new TestClass(),
				new TestClass(), // Will be squashed by field selection
				new TestClass
				{
					Bool = !TestBool
				}
			};

			var actual = testList.FieldSelect(fields).ToList();
			Assert.IsNotNull(actual);
			Assert.AreEqual(2, actual.Count);
		}

		[TestMethod]
		public void FieldSelectable_Multi_NoFields_NoDistinct()
		{
			var fields = new FieldSelectable("");
			var testList = new[]
			{
				new TestClass(),
				new TestClass(), // Will be squashed by field selection
				new TestClass
				{
					Bool = !TestBool
				}
			};

			var actual = testList.FieldSelect(fields).ToList();
			Assert.IsNotNull(actual);
			Assert.AreEqual(3, actual.Count);
		}

		private class FieldSelectable : IFieldSelectable
		{
			public FieldSelectable(string fields)
			{
				Fields = fields;
			}

			public string Fields { get; }
		}

		private class TestClass
		{
			public string String { get; set; } = TestString;
			public int Int { get; set; } = TestInt;
			public bool Bool { get; set; } = TestBool;
		}
	}
}
