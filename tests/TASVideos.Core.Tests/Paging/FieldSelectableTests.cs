﻿using TASVideos.Extensions;

namespace TASVideos.Core.Tests.Paging;

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
	public void FieldSelectable_Single_EmptySelect_ReturnsObj(string? fields)
	{
		var testClass = new TestClass();
		var actual = testClass.FieldSelect(fields);
		var dic = (IDictionary<string, object?>)actual;
		Assert.AreEqual(3, dic.Count);
		Assert.AreEqual(dic[nameof(TestClass.StringValue).PascalToCamelCase()], TestString);
		Assert.AreEqual(dic[nameof(TestClass.Int).PascalToCamelCase()], TestInt);
		Assert.AreEqual(dic[nameof(TestClass.Bool).PascalToCamelCase()], TestBool);
	}

	[TestMethod]
	public void FieldSelectable_Single_SingleSelect_ReturnsSingle()
	{
		var testClass = new TestClass();
		var actual = testClass.FieldSelect(nameof(TestClass.StringValue).PascalToCamelCase());
		var dic = (IDictionary<string, object?>)actual;
		Assert.AreEqual(1, dic.Count);
		Assert.AreEqual(dic[nameof(TestClass.StringValue).PascalToCamelCase()], TestString);

		Assert.IsFalse(dic.ContainsKey(nameof(TestClass.Int).PascalToCamelCase()));
		Assert.IsFalse(dic.ContainsKey(nameof(TestClass.Bool).PascalToCamelCase()));
	}

	[TestMethod]
	public void FieldSelectable_Single_DoubleSelect_ReturnsDouble()
	{
		const string fields = $"{nameof(TestClass.StringValue)},{nameof(TestClass.Int)}";
		var testClass = new TestClass();
		var actual = testClass.FieldSelect(fields);
		var dic = (IDictionary<string, object?>)actual;
		Assert.AreEqual(2, dic.Count);
		Assert.AreEqual(dic[nameof(TestClass.StringValue).PascalToCamelCase()], TestString);
		Assert.AreEqual(dic[nameof(TestClass.Int).PascalToCamelCase()], TestInt);

		Assert.IsFalse(dic.ContainsKey(nameof(TestClass.Bool).PascalToCamelCase()));
	}

	[TestMethod]
	public void FieldSelectable_Single_DoubleSelect_CanHaveSpaces_ReturnsDouble()
	{
		const string fields = $" {nameof(TestClass.StringValue)} , {nameof(TestClass.Int)} ";
		var testClass = new TestClass();
		var actual = testClass.FieldSelect(fields);
		var dic = (IDictionary<string, object?>)actual;
		Assert.AreEqual(2, dic.Count);
		Assert.AreEqual(dic[nameof(TestClass.StringValue).PascalToCamelCase()], TestString);
		Assert.AreEqual(dic[nameof(TestClass.Int).PascalToCamelCase()], TestInt);

		Assert.IsFalse(dic.ContainsKey(nameof(TestClass.Bool)));
	}

	[TestMethod]
	public void FieldSelectable_Multi_Distinct()
	{
		var fields = new FieldSelectable($"{nameof(TestClass.StringValue)},{nameof(TestClass.Bool)}");
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
		Assert.AreEqual(3, actual.Count);
	}

	private class FieldSelectable(string fields) : IFieldSelectable
	{
		public string Fields { get; } = fields;
	}

	private class TestClass
	{
		public string StringValue { get; init; } = TestString;
		public int Int { get; init; } = TestInt;
		public bool Bool { get; init; } = TestBool;
	}
}
