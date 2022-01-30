using System.Text;

namespace TASVideos.MovieParsers.Tests;

#pragma warning disable SA1117
#pragma warning disable SA1137
#pragma warning disable SA1515
[TestClass]
[TestCategory("JrsrParsers")]
public class JrsrTests : BaseParserTests
{
	private readonly Jrsr _jrsrParser;
	public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.JrsrSampleFiles.";

	public JrsrTests()
	{
		_jrsrParser = new Jrsr();
	}

	[TestMethod]
	public async Task EmptyFile()
	{
		var result = await _jrsrParser.Parse(Embedded("emptyfile.jrsr"));
		Assert.IsFalse(result.Success);
	}

	[TestMethod]
	public async Task CorrectMagic()
	{
		var result = await _jrsrParser.Parse(Embedded("correctmagic.jrsr"));
		Assert.IsTrue(result.Success);
		Assert.AreEqual(result.FileExtension, "jrsr");
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task WrongLineMagic()
	{
		var result = await _jrsrParser.Parse(Embedded("wronglinemagic.jrsr"));
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task WrongMagic()
	{
		var result = await _jrsrParser.Parse(Embedded("wrongmagic.jrsr"));
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task NoBeginHeader()
	{
		var result = await _jrsrParser.Parse(Embedded("nobeginheader.jrsr"));
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task Rerecords()
	{
		var result = await _jrsrParser.Parse(Embedded("correctmagic.jrsr"));
		Assert.IsTrue(result.Success);
		Assert.AreEqual(17984, result.RerecordCount);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task Savestate()
	{
		var result = await _jrsrParser.Parse(Embedded("savestate.jrsr"));
		Assert.IsTrue(result.Success);
		Assert.AreEqual(MovieStartType.Savestate, result.StartType);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task ContainsSavestate_ReturnError()
	{
		var result = await _jrsrParser.Parse(Embedded("containssavestate.jrsr"));
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task Frames()
	{
		var result = await _jrsrParser.Parse(Embedded("frames.jrsr"));
		Assert.IsTrue(result.Success);
		Assert.AreEqual(147789, result.Frames);
		Assert.AreEqual(60, result.FrameRateOverride);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task MissingRerecords()
	{
		var result = await _jrsrParser.Parse(Embedded("missingrerecords.jrsr"));
		Assert.IsTrue(result.Success);
		Assert.AreEqual(0, result.RerecordCount, "Rerecord count assumed to be 0");
		AssertNoErrors(result);
		Assert.AreEqual(1, result.Warnings.Count());
	}

	[TestMethod]
	public async Task NegativeRerecords()
	{
		var result = await _jrsrParser.Parse(Embedded("negativererecords.jrsr"));
		Assert.IsFalse(result.Success);
		Assert.AreEqual(-1, result.RerecordCount, "Rerecord count assumed to be -1");
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	/// <summary>
	/// Encodes <paramref name="contents"/> into UTF-8 and then parses it as
	/// JRSR.
	/// </summary>
	private static async Task<IParseResult> ParseFromString(string contents)
	{
		await using var reader = new MemoryStream(new UTF8Encoding(false, true).GetBytes(contents));
		return await new Jrsr().Parse(reader);
	}

	// It should be an error if RERECORDS appears more than once, even if
	// some of the appearances do not parse correctly.
	[TestMethod]
	[DataRow(
@"JRSR
!BEGIN header
+RERECORDS 100
+RERECORDS 100
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
+RERECORDS
+RERECORDS 100
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
+RERECORDS 100
+RERECORDS
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
+RERECORDS -123
+RERECORDS 100
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
+RERECORDS 100
+RERECORDS -123
!END
")]
	public async Task RerecordsMultiplicity(string contents)
	{
		var result = await ParseFromString(contents);
		Assert.IsFalse(result.Success);
	}

	[TestMethod]
	// No events section.
	[DataRow(
@"JRSR
!BEGIN header
!END
", 0)]
	// Events section may be empty.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
!END
", 0)]
	// Special events should not count towards frame count.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION ABSOLUTE
+1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+3333333400 SAVESTATE aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa 0
!END
", 100)]
	// Timestamps are absolute by default.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+3333333400 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
", 200)]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION RELATIVE
+1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+3333333400 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
", 300)]
	// OPTION takes effect after the line in which it appears.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+3333333400 OPTION RELATIVE
+0 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
", 300)]
	// Consecutive timestamps may be equal.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
", 100)]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION RELATIVE
+1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+0 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
", 100)]
	// Event parameters may be empty.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+1666666700 org.jpc.emulator.PC$ResetButton
!END
", 100)]
	// Just short of overflow in event timestamps. 9223372036854775807 is
	// 2**63 - 1. Use special events here to avoid conflation with overflow
	// that occurs while computing result.Frames.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+9223372036854775807 SAVESTATE aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa 0
!END
", 0)]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION RELATIVE
+9223372036854775806 SAVESTATE aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa 0
+1 SAVESTATE aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaab 1
!END
", 0)]
	// Just short of overflow in frame count. 35791394849161215 is
	// (2**31) * 16666667 - 1.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+35791394849161215 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
", 0x7fffffff)]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION RELATIVE
+35791394849161214 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+1 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
", 0x7fffffff)]
	public async Task EventTimestamps(string contents, int expected)
	{
		var result = await ParseFromString(contents);
		Assert.IsTrue(result.Success);
		Assert.AreEqual(expected, result.Frames);
	}

	[TestMethod]
	// More than one events section.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
!BEGIN events
+1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
")]
	// Missing parameter to OPTION.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION
!END
")]
	// Too many parameters to OPTION.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION RELATIVE ABSOLUTE
!END
")]
	// Bad parameter to OPTION.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION ERROR
!END
")]
	// Timestamps must be non-decreasing.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+3333333400 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+3333333399 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
")]
	// Timestamps are supposed to be non-negative, but check for
	// non-decreasing RELATIVE timestamps as well.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION RELATIVE
+3333333400 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+-1 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
")]
	// Unknown special event is an error.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 SPECIAL foobar
!END
")]
	// Do not permit whitespace, base prefixes, decimal points, etc. in
	// timestamps.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+( 1666666700 ) org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+\ 1666666700\  org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0x1666666700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+1666666700.0 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+1666666700e0 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+1,666,666,700 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
")]
	// Overflow in event timestamps. 9223372036854775808 is 2**63. Use
	// special events here to avoid conflation with overflow that occurs
	// while computing result.Frames.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+9223372036854775808 SAVESTATE aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa 0
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION RELATIVE
+9223372036854775807 SAVESTATE aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa 0
+1 SAVESTATE aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaab 1
!END
")]
	// Overflow in frame count. 35791394849161216 is (2**31) * 16666667.
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+35791394849161216 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
")]
	[DataRow(
@"JRSR
!BEGIN header
!BEGIN events
+0 OPTION RELATIVE
+35791394849161215 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
+1 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28
!END
")]
	public async Task EventTimestampsError(string contents)
	{
		var result = await ParseFromString(contents);
		Assert.IsFalse(result.Success);
	}

	/// <summary>
	/// Serializes the contents of a JRSR file into a flat array of strings.
	/// Each section name is followed by that section's lines, then a null
	/// element. The end of the final section is marked by an additional
	/// null element.
	/// </summary>
	private async Task<string?[]> Serialize(Stream reader, int lengthLimit = 10000)
	{
		// We will serialize the JRSR structure into a flat list of strings,
		// where each section name is followed by the section's lines, and
		// sections and the whole document are terminated by null.
		using var parser = await JrsrSectionParser.CreateAsync(reader, lengthLimit);
		var serialized = new List<string?>();
		while (await parser.NextSection() is { } sectionName)
		{
			serialized.Add(sectionName);
			while (await parser.NextLine() is { } line)
			{
				serialized.Add(line);
			}

			serialized.Add(null);
		}

		serialized.Add(null);
		return serialized.ToArray();
	}

	/// <summary>
	/// Like <see cref="Serialize"/>, but reads the JRSR from a string,
	/// which will be encoded to UTF-8 and wrapped in a <c>Stream</c>.
	/// </summary>
	private async Task<string?[]> SerializeFromString(string contents, int lengthLimit = 10000)
	{
		await using var reader = new MemoryStream(new UTF8Encoding(false, true).GetBytes(contents));
		return await Serialize(reader, lengthLimit);
	}

	[TestMethod]
	// It's permitted to have zero sections.
	[DataRow("JRSR\n", null)]
	// Sections may be empty.
	[DataRow(
@"JRSR
!BEGIN foo
!END
", "foo", null,
null)]
	[DataRow(
@"JRSR
!BEGIN foo
!END
!BEGIN bar
!END
", "foo", null,
"bar", null,
null)]
	// Uniqueness of section names is not checked a the JrsrSectionParser
	// level.
	[DataRow(
@"JRSR
!BEGIN foo
!END
!BEGIN bar
!END
!BEGIN foo
!END
", "foo", null,
"bar", null,
"foo", null,
null)]
	// !END is optional for non-final sections.
	[DataRow(
@"JRSR
!BEGIN foo
!BEGIN bar
!END
", "foo", null,
"bar", null,
null)]
	// Section names may contain spaces, including trailing spaces.
	[DataRow(
"JRSR\n" +
"!BEGIN\x20section\x20name\x20\x20\n" +
"!END\n" +
"", "section\x20name\x20\x20", null,
null)]
	// Non-ASCII section names and line contents.
	[DataRow(
"JRSR\n" +
"!BEGIN section \u2603\n" +
"+line \u2603\n" +
"!BEGIN section \U0001f427\n" +
"+line \U0001f427\n" +
"!BEGIN section \u2800\n" +
"+line \u2800\n" +
"!BEGIN section \u2c00\n" +
"+line \u2c00\n" +
"!END\n" +
"", "section \u2603", "line \u2603", null,
"section \U0001f427", "line \U0001f427", null,
"section \u2800", "line \u2800", null,
"section \u2c00", "line \u2c00", null,
null)]
	// Blank lines are permitted anywhere after magic.
	[DataRow(
@"JRSR

!BEGIN foo

+line 1


+line 2

!END
!BEGIN bar
+line 1
!END


", "foo", "line 1", "line 2", null,
"bar", "line 1", null,
null)]
	// All space and linefeed characters.
	[DataRow(
"JRSR\u000a" +
"!BEGIN\u0009\u000c\u0020foo\u000d" +
"+line\u1680\u180e1\u2028\u205f\u001c" +
"+line\u3000\u20002\u2001\u2002\u001d" +
"+line\u2003\u20043\u2005\u2006\u001e" +
"+line\u2007\u20084\u2009\u200a\u0085" +
"!END\u2029" +
"", "foo",
	"line\u1680\u180e1\u2028\u205f",
	"line\u3000\u20002\u2001\u2002",
	"line\u2003\u20043\u2005\u2006",
	"line\u2007\u20084\u2009\u200a",
	null,
null)]
	// Lines may contain section delimiters.
	[DataRow(
@"JRSR
!BEGIN foo
+!BEGIN bar
+!END
!END
", "foo", "!BEGIN bar", "!END", null,
null)]
	// Lines may have leading and trailing whitespace.
	[DataRow(
"JRSR\n" +
"!BEGIN foo\n" +
"+\x20\x20hello\x20world\x20\x20\n" +
"!END\n" +
"", "foo", "\x20\x20hello\x20world\x20\x20", null,
null)]
	// Section names that differ in case or Unicode normalization are
	// considered distinct.
	[DataRow(
"JRSR\n" +
"!BEGIN section\n" +
"!BEGIN Section\n" +
"!BEGIN SECTION\n" +
"!BEGIN \u00e9tude\n" +
"!BEGIN e\u0301tude\n" +
"!END\n" +
"", "section", null,
"Section", null,
"SECTION", null,
"\u00e9tude", null,
"e\u0301tude", null,
null)]
	public async Task ParserGood(string contents, params string?[] expected)
	{
		CollectionAssert.AreEqual(expected, await SerializeFromString(contents));
	}

	[TestMethod]
	[ExpectedException(typeof(FormatException))]
	// Missing magic.
	[DataRow("")]
	[DataRow(
@"!BEGIN foo
!END
")]
	// No whitespace permitted before magic.
	[DataRow(
@"
JRSR
!BEGIN foo
!END
")]
	[DataRow(
@" JRSR
!BEGIN foo
!END
")]
	// No BOM permitted before magic.
	[DataRow(
"\ufeffJRSR\n" +
"!BEGIN foo\n" +
"!END\n" +
"")]
	// Need whitespace after !BEGIN.
	[DataRow(
@"JRSR
!BEGIN")]
	// Need a section name after !BEGIN.
	[DataRow(
@"JRSR
!BEGIN ")]
	[DataRow(
"JRSR\n" +
"!BEGIN \n" +
"")]
	// Need a linefeed after section name
	[DataRow(
@"JRSR
!BEGIN foo")]
	// Need !END after the final !BEGIN.
	[DataRow(
@"JRSR
!BEGIN foo
")]
	[DataRow(
@"JRSR
!BEGIN foo
!END
!BEGIN bar
")]
	// Need linefeed after !END.
	[DataRow(
@"JRSR
!BEGIN foo
!END ")]
	[DataRow(
"JRSR\n" +
"!BEGIN foo\n" +
"!END \n" +
"")]
	[DataRow(
@"JRSR
!BEGIN foo
!END foo
")]
	// Need !BEGIN before !END.
	[DataRow(
@"JRSR
!END
")]
	// Expect BEGIN or END after !.
	[DataRow(
@"JRSR
!ERROR
")]
	[DataRow(
@"JRSR
!BEGIN foo
!ERROR
!END
")]
	// Lines must be in a section.
	[DataRow(
@"JRSR
+line
!BEGIN foo
!END
")]
	[DataRow(
@"JRSR
!BEGIN foo
!END
+line
")]
	public async Task ParserFormatException(string contents)
	{
		await SerializeFromString(contents);
	}

	[TestMethod]
	[ExpectedException(typeof(FormatException))]
	public async Task ParserDecoderUtf16()
	{
		// Input not encoded as UTF-8 should be an error.
		var contents = @"JRSR
!BEGIN foo
+line
!END
";
		await using var reader = new MemoryStream(Encoding.Unicode.GetBytes(contents));
		await Serialize(reader);
	}

	[TestMethod]
	[ExpectedException(typeof(FormatException))]
	public async Task ParserDecoderUtf8Error()
	{
		// Input with UTF-8 encoding errors should be an error. We must
		// paste the input together ourselves because
		// System.Text.UTF8Encoding refuses to encode surrogates.
		var enc = new UTF8Encoding(false, true);
		var contents = enc.GetBytes("JRSR\n!BEGIN foo\n+")
			.Concat(new byte[] { 0xed, 0xa0, 0x80 }) // UTF-8 encoding of the surrogate '\ud800'
			.Concat(enc.GetBytes("\n!END\n"))
			.ToArray();
		await using var reader = new MemoryStream(contents);
		await Serialize(reader);
	}

	[TestMethod]
	public async Task ParserDecoderNavigation()
	{
		var contents =
@"JRSR
!BEGIN section 1
+line 1.1
+line 1.2
+line 1.3
+line 1.4
+line 1.5
!END
!BEGIN section 2
+line 2.1
!END
";
		await using var reader = new MemoryStream(new UTF8Encoding(false, true).GetBytes(contents));
		using var parser = await JrsrSectionParser.CreateAsync(reader, 10000);

		// Cannot call NextLine before the first section.
		await Assert.ThrowsExceptionAsync<InvalidOperationException>(parser.NextLine);

		Assert.AreEqual("section 1", await parser.NextSection());
		Assert.AreEqual("line 1.1", await parser.NextLine());
		Assert.AreEqual("line 1.2", await parser.NextLine());

		// Should skip over the remaining lines in section 1.
		Assert.AreEqual("section 2", await parser.NextSection());
		Assert.AreEqual("line 2.1", await parser.NextLine());
		Assert.IsNull(await parser.NextLine());

		// The result of NextLine should remain null.
		Assert.IsNull(await parser.NextLine());

		Assert.IsNull(await parser.NextSection());

		// Cannot call NextLine after the final section.
		await Assert.ThrowsExceptionAsync<InvalidOperationException>(parser.NextLine);

		// The result of NextSection should remain null.
		Assert.IsNull(await parser.NextSection());
	}

	[TestMethod]
	public async Task ParserDecoderLengthLimit()
	{
		{
			var contents =
@"JRSR
!BEGIN a
+1
!BEGIN b
+2
!END
";
			var expected = new[]
			{
					"a", "1", null,
					"b", "2", null,
					null,
				};
			await Assert.ThrowsExceptionAsync<FormatException>(() => SerializeFromString(contents, -1));
			await Assert.ThrowsExceptionAsync<FormatException>(() => SerializeFromString(contents, 0));
			CollectionAssert.AreEqual(expected, await SerializeFromString(contents, 1));
			CollectionAssert.AreEqual(expected, await SerializeFromString(contents, 2));
		}

		{
			var contents =
@"JRSR
!BEGIN a
+1
+11
+111
+1111
+11111
!END
";
			var expected = new[]
			{
					"a", "1", "11", "111", "1111", "11111", null,
					null,
				};
			await Assert.ThrowsExceptionAsync<FormatException>(() => SerializeFromString(contents, 4));
			CollectionAssert.AreEqual(expected, await SerializeFromString(contents, 5));
		}

		{
			var contents =
@"JRSR
!BEGIN a
!BEGIN aa
!BEGIN aaa
!BEGIN aaaa
!BEGIN aaaaa
!END
";
			var expected = new[]
			{
					"a", null,
					"aa", null,
					"aaa", null,
					"aaaa", null,
					"aaaaa", null,
					null,
				};
			await Assert.ThrowsExceptionAsync<FormatException>(() => SerializeFromString(contents, 4));
			CollectionAssert.AreEqual(expected, await SerializeFromString(contents, 5));
		}
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("\u0009\u0020\u1680\u180e\u2028\u205f\u3000\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200a")]
	[DataRow("foo", "foo")]
	[DataRow("foo\\\\", "foo\\")]
	[DataRow("foo\U0001f427bar", "foo\U0001f427bar")]
	[DataRow("foo\\\U0001f427bar", "foo\U0001f427bar")]
	[DataRow("foo\ud800bar", "foo\ud800bar")]
	[DataRow("foo\\\ud800bar", "foo\ud800bar")]
	[DataRow("foo\udc00bar", "foo\udc00bar")]
	[DataRow("foo\\\udc00bar", "foo\udc00bar")]
	[DataRow("() ()() (()) (()())", "()", "()()")]
	[DataRow("(hello(\\(world))((hello\\))world)", "hello((world)", "(hello))world")]
	[DataRow("hello\\ world", "hello world")]
	[DataRow("hello\\\u3000world", "hello\u3000world")]
	[DataRow("hello\\\\world", "hello\\world")]
	[DataRow("DISKNAME 0(FreeDOS (initial fda disk))", "DISKNAME", "0", "FreeDOS (initial fda disk)")]
	[DataRow("COMMENT(Entry: 19900101000000 10ac35dd6bc6314cd5caf08a4ffb4275      76586 /DAVE.EXE)", "COMMENT", "Entry: 19900101000000 10ac35dd6bc6314cd5caf08a4ffb4275      76586 /DAVE.EXE")]
	// The following contain linefeeds and cannot actually appear in the
	// context of a JRSR file.
	[DataRow("hello\\\u000aworld", "hello\u000aworld")]
	[DataRow("hello\u000aworld", "hello\u000aworld")]
	public void DecodeComponentGood(string line, params string[] expected)
	{
		CollectionAssert.AreEqual(expected, JrsrSectionParser.DecodeComponent(line).ToList());
	}

	[TestMethod]
	[ExpectedException(typeof(FormatException))]
	// Unmatched '('.
	[DataRow("(foo")]
	[DataRow("(foo(bar)baz")]
	// Unmatched ')'.
	[DataRow("foo)")]
	[DataRow("foo(bar)baz)")]
	// Backslash at end of string.
	[DataRow("foo\\")]
	public void DecodeComponentFormatException(string line)
	{
		// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
		JrsrSectionParser.DecodeComponent(line).ToList();
	}

	[TestMethod]
	[DataRow("", false)]
	[DataRow("OPTION", true)]
	[DataRow("SAVESTATE", true)]
	[DataRow("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", true)]
	[DataRow("org.jpc.emulator.peripheral.Keyboard", false)]
	[DataRow("OPTION SAVESTATE", false)]
	[DataRow("OPTIO\u0418", false)] // Capital letter but not ASCII.
	public void IsSpecialEventClass(string eventClass, bool expected)
	{
		Assert.AreEqual(expected, JrsrSectionParser.IsSpecialEventClass(eventClass));
	}
}
