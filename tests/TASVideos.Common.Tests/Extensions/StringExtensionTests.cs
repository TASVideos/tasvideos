using TASVideos.Extensions;

namespace TASVideos.Common.Tests.Extensions;

[TestClass]
public class StringExtensionTests
{
	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void CapAndEllipse_NegativeLimit_Throws()
	{
		"".CapAndEllipse(-1);
	}

	[TestMethod]
	[DataRow(null, 0, "")]
	[DataRow(null, 1, "")]
	[DataRow("", 0, "")]
	[DataRow("", 1, "")]
	[DataRow("1234", 1, ".")]
	[DataRow("1234", 2, "..")]
	[DataRow("123", 3, "123")]
	[DataRow("1234", 4, "1234")]
	[DataRow("123456789", 7, "1234...")]
	[DataRow("123456789", 8, "12345...")]
	[DataRow("123456789", 9, "123456789")]
	[DataRow("123456789", 15, "123456789")]
	[DataRow("🤣🤣🤣🤣🤣🤣", 8, "🤣🤣🤣...")]
	public void CapAndEllipse_Tests(string str, int limit, string expected)
	{
		var actual = str.CapAndEllipse(limit);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, "")]
	[DataRow("", "")]
	[DataRow("\r \n \t", "\r \n \t")]
	[DataRow(" ", " ")]
	[DataRow("onlylowercase", "onlylowercase")]
	[DataRow(" trimspaces ", "trimspaces")]
	[DataRow("If Spaces Do Not Add Extra Spaces", "If Spaces Do Not Add Extra Spaces")]
	[DataRow("HelloWorld", "Hello World")]
	[DataRow("ABTest", "ABTest")]
	[DataRow("ABCTest", "ABCTest")]
	[DataRow("PCem", "PCem")]
	[DataRow("TASVideos", "TASVideos")]
	[DataRow("NumbersGet1Space", "Numbers Get 1 Space")]
	[DataRow("Special.Characters.Get.Spaces", "Special . Characters . Get . Spaces")]
	[DataRow("GameResources/NES/SuperMarioBros", "Game Resources / NES / Super Mario Bros")]
	[DataRow("HomePages/adelikat", "Home Pages / adelikat")]
	[DataRow("HomePages/Adelikat", "Home Pages / Adelikat")]
	[DataRow("HomePages/[^_^]", "Home Pages / [^_^]")]
	public void SplitCamelCase_Tests(string str, string expected)
	{
		var actual = str.SplitCamelCase();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow(",")]
	[DataRow(" , ")]
	[DataRow(",,,")]
	public void CsvToString_NullReturnsEmpty(string str)
	{
		var actual = str.CsvToStrings();
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	[DataRow("A", "A")]
	[DataRow("A,B", "A", "B")]
	[DataRow("A ,B ", "A", "B")]
	public void CsvToStrings(string str, params string[] expected)
	{
		var actual = str.CsvToStrings();
		Assert.IsNotNull(actual);
		Assert.IsTrue(expected.OrderBy(e => e).SequenceEqual(actual.OrderBy(a => a)));
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow(",")]
	[DataRow(" , ")]
	[DataRow(",,,")]
	public void CsvToInts_NullReturnsEmpty(string str)
	{
		var actual = str.CsvToInts();
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	[DataRow("1", new[] { 1 })]
	[DataRow("1,2", new[] { 1, 2 })]
	[DataRow("1 ,2 ", new[] { 1, 2 })]
	[DataRow("-1,1", new[] { -1, 1 })]
	[DataRow("1,2,NotANumber", new[] { 1, 2 })]
	public void CsvToInts(string str, int[] expected)
	{
		var actual = str.CsvToInts();
		Assert.IsNotNull(actual);
		Assert.IsTrue(expected.OrderBy(e => e).SequenceEqual(actual.OrderBy(a => a)));
	}

	[DataRow("abcd", 2, "cd")]
	[DataRow("abcd", 4, "")]
	[DataRow("abcd", 0, "abcd")]
	[DataRow("", 0, "")]
	[DataRow("abc", -1, null)]
	[DataRow("abc", 10, null)]
	[DataRow("ROFFAL 🤣🤣🤣", 7, "🤣🤣🤣")]
	[DataRow("ROFFAL 🤣🤣🤣", 8, "🤣🤣🤣")]
	[TestMethod]
	public void UnicodeAwareSubstring1(string s, int i, string? expected)
	{
		if (expected == null)
		{
			var threw = false;
			try
			{
				s.UnicodeAwareSubstring(i);
			}
			catch
			{
				threw = true;
			}

			Assert.IsTrue(threw);
		}
		else
		{
			var actual = s.UnicodeAwareSubstring(i);
			Assert.AreEqual(expected, actual);
		}
	}

	[DataRow("abcd", 2, 1, "c")]
	[DataRow("abcd", 4, 0, "")]
	[DataRow("abcd", 0, 2, "ab")]
	[DataRow("", 0, 0, "")]
	[DataRow("abc", -1, 0, null)]
	[DataRow("abc", 10, 0, null)]
	[DataRow("abcd", 0, 5, null)]
	[DataRow("ROFFAL 🤣🤣🤣", 7, 4, "🤣🤣")]
	[DataRow("ROFFAL 🤣🤣🤣", 8, 2, "🤣🤣")]
	[TestMethod]
	public void UnicodeAwareSubstring2(string s, int i, int j, string? expected)
	{
		if (expected == null)
		{
			var threw = false;
			try
			{
				s.UnicodeAwareSubstring(i, j);
			}
			catch
			{
				threw = true;
			}

			Assert.IsTrue(threw);
		}
		else
		{
			var actual = s.UnicodeAwareSubstring(i, j);
			Assert.AreEqual(expected, actual);
		}
	}

	[DataRow(null, null)]
	[DataRow("", null)]
	[DataRow(" ", null)]
	[DataRow("\n", null)]
	[DataRow("Test", "Test")]
	[DataRow("Test\n", "Test\n")]
	[TestMethod]
	public void NullIfWhitespace(string s, string expected)
	{
		var actual = s.NullIfWhitespace();
		Assert.AreEqual(expected, actual);
	}
}
