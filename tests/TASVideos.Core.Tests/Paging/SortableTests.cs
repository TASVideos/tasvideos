﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TASVideos.Core.Tests.Paging;

[TestClass]
public class SortableTests
{
	[TestMethod]
	public void SortableIsSortingParam_NullSafe()
	{
		var sortable = (ISortable?)null;

		// ReSharper disable once ExpressionIsAlwaysNull
		var actual = sortable.IsSortingParam("Param");
		Assert.IsFalse(actual);
	}

	[TestMethod]
	[DataRow(null, null, false, DisplayName = "Null param always false")]
	[DataRow("", null, false, DisplayName = "Null param always false")]
	[DataRow(null, "", false, DisplayName = "Empty param always false")]
	[DataRow("", "", false, DisplayName = "Empty param always false")]
	[DataRow(null, "\r\n ", false, DisplayName = "Whitespace param always false")]
	[DataRow("", "\r\n ", false, DisplayName = "Whitespace param always false")]
	[DataRow(",", "", false)]
	[DataRow(" , ", " ", false)]
	[DataRow("", "Param", false)]
	[DataRow("Param", "Param", true)]
	[DataRow("+Param", "Param", true)]
	[DataRow("-Param", "Param", true)]
	[DataRow("-Param", "param", true, DisplayName = "Case insensitive")]
	[DataRow("-param", "param", true, DisplayName = "Case insensitive")]
	[DataRow("-parAm", "paRam", true, DisplayName = "Case insensitive")]
	[DataRow(" param ", "param", true, DisplayName = "Spaces")]
	public void Sortable_IsSortingParamTests(string? sortStr, string? param, bool expected)
	{
		var sortable = new Sortable(sortStr);
		var actual = sortable.IsSortingParam(param);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void SortableIsDescending_NullSafe()
	{
		var sortable = (ISortable?)null;

		// ReSharper disable once ExpressionIsAlwaysNull
		var actual = sortable.IsDescending("Param");
		Assert.IsFalse(actual);
	}

	[TestMethod]
	[DataRow(null, null, false, DisplayName = "Null param always false")]
	[DataRow("", null, false, DisplayName = "Null param always false")]
	[DataRow(null, "", false, DisplayName = "Empty param always false")]
	[DataRow("", "", false, DisplayName = "Empty param always false")]
	[DataRow(null, "\r\n ", false, DisplayName = "Whitespace param always false")]
	[DataRow("", "\r\n ", false, DisplayName = "Whitespace param always false")]
	[DataRow(",", "", false)]
	[DataRow(" , ", " ", false)]
	[DataRow("", "Param", false)]
	[DataRow("Param", "Param", false)]
	[DataRow("+Param", "Param", false)]
	[DataRow("-Param", "Param", true)]
	[DataRow("-Param", "param", true, DisplayName = "Case insensitive")]
	[DataRow("-param", "param", true, DisplayName = "Case insensitive")]
	[DataRow("-parAm", "paRam", true, DisplayName = "Case insensitive")]
	[DataRow("+Param", "Param", false, DisplayName = "Spaces")]
	public void Sortable_IsDescendingTests(string? sortStr, string? param, bool expected)
	{
		var sortable = new Sortable(sortStr);
		var actual = sortable.IsDescending(param);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public void Sortable_IsValidSort_NullSafe()
	{
		var sortable = (ISortable?)null;

		var actual = sortable.IsValidSort(typeof(string));
		Assert.IsTrue(actual);
	}

	[TestMethod]
	[DataRow("Foo", null, false, DisplayName = "Null type always false")]
	[DataRow(null, typeof(string), true, DisplayName = "Null or whitespace sorts considered true")]
	[DataRow("", typeof(string), true, DisplayName = "Null or whitespace sorts considered true")]
	[DataRow("\r \n \t", typeof(string), true, DisplayName = "Null or whitespace sorts considered true")]
	[DataRow(",,,", typeof(string), true, DisplayName = "Empty sorts are ignored")]
	[DataRow("", typeof(TestResponse), true)]
	[DataRow("Foo", typeof(TestResponse), true)]
	[DataRow("foo", typeof(TestResponse), true)]
	[DataRow("FoO", typeof(TestResponse), true)]
	[DataRow("Bar", typeof(TestResponse), false)]
	[DataRow("DoesNotExist", typeof(TestResponse), false)]
	[DataRow("Foo,Baz", typeof(TestResponse), true)]
	[DataRow(" Foo , Baz ", typeof(TestResponse), true)]
	[DataRow("Foo,Bar,Baz", typeof(TestResponse), false)]
	public void Sortable_IsValidSort_Tests(string? sortStr, Type? type, bool expected)
	{
		var sortable = new Sortable(sortStr);
		var actual = sortable.IsValidSort(type);
		Assert.AreEqual(expected, actual);
	}

	private class Sortable : ISortable
	{
		public Sortable(string? sort)
		{
			Sort = sort;
		}

		public string? Sort { get; }
	}

	private class TestResponse
	{
		[Sortable]

		// ReSharper disable once UnusedMember.Global
		// ReSharper disable once UnusedMember.Local
		public string Foo { get; set; } = "";

		// ReSharper disable once UnusedMember.Global
		// ReSharper disable once UnusedMember.Local
		public int Bar { get; set; }

		[Sortable]

		// ReSharper disable once UnusedMember.Global
		// ReSharper disable once UnusedMember.Local
		public bool Baz { get; set; }
	}
}
