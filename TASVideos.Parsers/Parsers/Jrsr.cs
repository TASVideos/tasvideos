using System.Globalization;
using System.Text;
using TASVideos.MovieParsers.Result;

/*
 * https://tasvideos.org/EmulatorResources/JPC/JRSRFormat
 *
 * Abstractly, a JRSR file is a container for a list of named sections. Each
 * section contains a list of lines. Section names must be unique. The order of
 * section names does not matter (think of it as a mapping). Inside a section,
 * the order of lines matters.
 *
 * The format of a line depends on the name of the section it appears in. Most
 * commonly, lines are in "component" format, which encodes a list of zero or
 * more non-empty text tokens.
 *
 * The internal class JrsrSectionParser is a one-pass, memory-bounded parser for
 * the syntactic elements of JRSR. It provides a way to incrementally read
 * section names and lines within sections, without interpreting them. The
 * private method JrsrSectionParser.DecodeComponent iterates over the tokens of
 * a line in "component" format.
 *
 * The public class Jrsr interprets the semantics of the JRSR structure; i.e.,
 * it looks for specific section names and understands the format of the lines
 * within them.
 */

/*
 * JRSR files are Unicode text, encoded in UTF-8. This is not a perfect match
 * for System.IO.StreamReader and the char data type, which deal not in whole
 * Unicode code points or Unicode scalar values, but in UTF-16 code units. Code
 * points outside the basic multilingual plane (BMP; >= 0x10000) are represented
 * not as a single char but as a pair of surrogates.
 *
 * The really correct thing to do would be to decode the input into a sequence
 * of System.Text.Rune (which represents a whole Unicode scalar value), but
 * StreamReader and char are actually sufficient for our purposes. This is
 * because all the syntax elements we care about (spaces and linefeeds, '+',
 * '\', '(', ')', keywords "JRSR", "!BEGIN", "!END") consist only of characters
 * from the BMP. Anything that is true or false of these characters is also true
 * or false of the UTF-16 code units that represent them; anything that is true
 * or false of code points outside the BMP is also true or false of all
 * surrogates.
 */

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("jrsr")]
public class Jrsr : IParser
{
	// Safety limit on the length of JRSR section names and lines.
	private const int LengthLimit = 10000;

	// Permissible formats for parsing integers. "All integers are written
	// using Unicode codepoints DIGIT ZERO - DIGIT NINE (and prefixed with
	// HYPHEN-MINUS if negative)." We want to exclude things like leading
	// and trailing whitespace. AllowLeadingSign permits a '+' prefix as
	// well as '-', but JPC-RR uses e.g. Java's Long.parseLong, which also
	// permits a '+' prefix.
	private const NumberStyles IntegerStyle = NumberStyles.AllowLeadingSign;

	private const string FileExtension = "jrsr";
	public async Task<IParseResult> Parse(Stream file)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.Dos
		};

		var sectionsSeen = new HashSet<string>();
		bool hasRerecordCount = false;
		long lastTimestamp = 0L;
		long lastNonSpecialTimestamp = 0L;
		bool optionRelative = false; // "By default [timestamps] are relative to initial power-on."
		try
		{
			using var parser = await JrsrSectionParser.CreateAsync(file, LengthLimit);
			while (await parser.NextSection() is { } sectionName)
			{
				if (sectionsSeen.Contains(sectionName))
				{
					throw new FormatException($"Duplicate section {sectionName}");
				}

				sectionsSeen.Add(sectionName);

				if (sectionName == "savestate")
				{
					return new ErrorResult("File contains a savestate");
				}

				if (sectionName == "header")
				{
					// https://tasvideos.org/EmulatorResources/JPC/JRSRFormat#HeaderSection
					while (await parser.NextLine() is { } line)
					{
						var tokens = JrsrSectionParser.DecodeComponent(line).ToList();
						if (tokens.Count < 1)
						{
							continue;
						}

						if (tokens[0] == Keys.RerecordCount)
						{
							if (hasRerecordCount)
							{
								throw new FormatException($"More than one {sectionName}.{tokens[0]} line");
							}

							hasRerecordCount = true;

							if (tokens.Count != 2)
							{
								throw new FormatException($"Bad format for {sectionName}.{tokens[0]} line");
							}

							if (!int.TryParse(tokens[1], IntegerStyle, null, out var rerecordValue) || rerecordValue < 0)
							{
								throw new FormatException($"Invalid {sectionName}.{tokens[0]} count {tokens[1]}");
							}

							result.RerecordCount = rerecordValue;
						}
						else if (tokens[0] == Keys.StartsFromSavestate)
						{
							result.StartType = MovieStartType.Savestate;
						}
					}
				}
				else if (sectionName == "events")
				{
					// https://tasvideos.org/EmulatorResources/JPC/JRSRFormat#EventsSection
					while (await parser.NextLine() is { } line)
					{
						var tokens = JrsrSectionParser.DecodeComponent(line).ToList();
						if (tokens.Count < 1)
						{
							continue;
						}

						if (tokens.Count < 2)
						{
							throw new FormatException("Missing event timestamp and class");
						}

						if (!long.TryParse(tokens[0], IntegerStyle, null, out var timestamp) || timestamp < 0)
						{
							throw new FormatException($"Cannot parse timestamp {tokens[0]}");
						}

						if (optionRelative)
						{
							timestamp = checked(lastTimestamp + timestamp);
						}

						// "Events must be in time order from first to last."
						if (timestamp < lastTimestamp)
						{
							throw new FormatException($"Event timestamp {timestamp} is before preceding timestamp {lastTimestamp}");
						}

						lastTimestamp = timestamp;

						var eventClass = tokens[1];
						if (eventClass == "OPTION")
						{
							if (tokens.Count != 3)
							{
								throw new FormatException($"Bad format for {eventClass} special event");
							}

							if (tokens[2] == "RELATIVE")
							{
								optionRelative = true;
							}
							else if (tokens[2] == "ABSOLUTE")
							{
								optionRelative = false;
							}
							else
							{
								throw new FormatException($"Unknown {eventClass} parameter {tokens[2]}");
							}
						}
						else if (eventClass == "SAVESTATE")
						{
							// Just check the syntax.
							if (tokens.Count != 4)
							{
								throw new FormatException($"Bad format for {eventClass} special event");
							}
						}
						else if (JrsrSectionParser.IsSpecialEventClass(eventClass))
						{
							throw new FormatException($"Unknown special event class {eventClass}");
						}
						else
						{
							lastNonSpecialTimestamp = lastTimestamp;
						}
					}
				}

				// initialization, diskinfo-*, others are ignored.
			}

			// "When computing movie length, it is customary to ignore all
			// special events."
			if (lastNonSpecialTimestamp > 0)
			{
				checked
				{
					result.Frames = (int)(lastNonSpecialTimestamp / 16666667);
					result.FrameRateOverride = result.Frames / (lastNonSpecialTimestamp / 1000000000L);
				}
			}
		}
		catch (FormatException ex)
		{
			return new ErrorResult(ex.Message);
		}
		catch (OverflowException ex)
		{
			return new ErrorResult(ex.Message);
		}

		if (!sectionsSeen.Contains("header"))
		{
			return new ErrorResult("No header found");
		}

		if (!hasRerecordCount)
		{
			result.WarnNoRerecords();
		}

		return result;
	}

	private static class Keys
	{
		public const string RerecordCount = "RERECORDS";
		public const string StartsFromSavestate = "SAVESTATEID";
	}
}

/// <summary>
/// Parser for the structure of a JRSR file. Call <see cref="NextSection"/>
/// to get the name of the next section, or <c>null</c> if there are no more
/// sections. While in a section, call <see cref="NextLine"/> to get the
/// next line, or <c>null</c> if there are no more lines in the section.
/// This class does not check or enforce uniqueness of section names.
/// </summary>
/// <example>
/// <code>
/// using (var parser = JrsrSectionParser.CreateAsync(stream, 10000))
/// {
///     while (await parser.NextSection() is string sectionName)
///     {
///         if (sectionName == "events")
///         {
///             while (await parser.NextLine() is string line)
///             {
///                 var tokens = JrsrSectionParser.DecodeComponent(line).ToList();
///             }
///         }
///     }
/// }
/// </code>
/// </example>
internal class JrsrSectionParser : IDisposable
{
	/*
	* https://tasvideos.org/EmulatorResources/JPC/JRSRFormat#FileStructure
	* https://repo.or.cz/jpcrr.git/blob/6ab255ce10b2dd10b0ac3ab2e2be708e0b26eeaa:/org/jpc/jrsr/JRSRArchiveReader.java#l714
	*
	* Grammar for JRSR sections and lines:
	*   space ::= [#x09#x20#x1680#x180e#x2000-#x200a#x2028#x205f#x3000]
	*   lf    ::= [#x0a#x0d#x1c-#x1e#x85#x2029]
	*   nonlf ::= [^#x0a#x0d#x1c-#x1e#x85#x2029]
	*   jrsr  ::= "JRSR" lf+ sections?
	*   begin ::= "!BEGIN" space+ nonlf+ lf+
	*   line  ::= "+" nonlf+ lf+
	*   end   ::= "!END" lf+
	*   sections ::= section-begin
	*                line*
	*                (section-end?
	*                 section-begin
	*                 line*)*
	*                section-end
	*/

	private readonly StreamReader _reader;

	// A safety limit on the length of section names and the length of lines
	// within sections. The JRSR format does not otherwise limit the length
	// of these strings.
	private readonly int _lengthLimit;

	private bool _inSection;

	// We will need to read 1 char at a time from the stream, and also to be
	// able to "unread" up to 1 char so that it can be read again later.
	private readonly char[] _readBuf = new char[1];
	private char? _unread;

	/// <summary>
	/// If a character was previously unread using <see cref="UnreadChar"/>,
	/// returns it an clears the unread buffer. Otherwise, reads and returns
	/// the next character from <see cref="_reader"/>. Returns -1 at the end
	/// of the stream.
	/// </summary>
	/// <exception name="FormatException">The next bytes in the stream do
	/// not encode a character in UTF-8.</exception>
	private async Task<int> ReadChar()
	{
		if (_unread is { } c)
		{
			// If we previously unread a character, return it now.
			_unread = null;
			return c;
		}

		// Otherwise, read a new character from _reader.
		try
		{
			var n = await _reader.ReadBlockAsync(_readBuf, 0, _readBuf.Length);
			return n == 0 ? -1 : _readBuf[0];
		}
		catch (DecoderFallbackException ex)
		{
			throw new FormatException("Decode", ex);
		}
	}

	/// <summary>
	/// Stores a <see cref="c"/> in the unread buffer, to be returned by a
	/// future call to <see cref="ReadChar"/>.
	/// </summary>
	/// <exception name="InvalidOperationException">The method was called
	/// twice in a row, without an intervening <see cref="ReadChar"/> to
	/// clear the unread buffer.</exception>
	private void UnreadChar(char c)
	{
		if (_unread is not null)
		{
			throw new InvalidOperationException("UnreadChar called twice without intervening ReadChar");
		}

		_unread = c;
	}

	// The constructor is private, only accessible through the public
	// factory method CreateAsync.
	private JrsrSectionParser(StreamReader reader, int lengthLimit)
	{
		_reader = reader;
		_lengthLimit = lengthLimit;
	}

	public void Dispose() => _reader.Dispose();

	/// <summary>
	/// Creates a new instance of <see cref="JrsrSectionParser"/>.
	/// </summary>
	/// <param name="stream">The <c>Stream</c> to read from.</param>
	/// <param name="lengthLimit">A limit on the length of section names and
	/// line. Strings longer than this result in <c>FormatException</c>, as
	/// if there had been a syntax error.</param>
	/// <exception name="FormatException">The file header is missing the
	/// JRSR magic.</exception>
	/// <exception name="DecoderFallbackException">The file is incorrectly
	/// encoded as UTF-8.</exception>
	public static async Task<JrsrSectionParser> CreateAsync(Stream stream, int lengthLimit)
	{
		// https://tasvideos.org/EmulatorResources/JPC/JRSRFormat#FileConventions
		// "The character set is always UTF-8."
		// encoderShouldEmitUTF8Identifier=false means that the StreamReader
		// will retain a UTF-8 byte order mark "\xef\xbb\xbf" if present at
		// the beginning of the Stream--we want to detect that as a syntax
		// error. throwOnInvalidBytes=true means to throw a
		// DecoderFallbackException for badly encoded UTF-8, rather than
		// substituting the replacement character U+FFFD.
		var reader = new StreamReader(stream, new UTF8Encoding(false, true), false);

		try
		{
			// Check magic.
			// https://tasvideos.org/EmulatorResources/JPC/JRSRFormat#Magic
			var magic = new char[5];
			var n = await reader.ReadBlockAsync(magic, 0, magic.Length);
			if (!(n == 5 && new string(magic[..4]) == "JRSR" && IsLinefeed(magic[4])))
			{
				throw new FormatException("Missing magic");
			}
		}
		catch (DecoderFallbackException ex)
		{
			throw new FormatException("Decode", ex);
		}

		return new JrsrSectionParser(reader, lengthLimit);
	}

	/// <summary>
	/// Returns whether <paramref name="c"/> is a JRSR space character. This
	/// method returns false for all surrogates.
	/// </summary>
	private static bool IsSpace(char c)
	{
		return c switch
		{
			// https://tasvideos.org/EmulatorResources/JPC/JRSRFormat#Spaces
			// https://repo.or.cz/jpcrr.git/blob/6ab255ce10b2dd10b0ac3ab2e2be708e0b26eeaa:/org/jpc/Misc.java#l100
			// The tasvideos.org documentation, as of Revision 7, is
			// missing one character from the reference implementation,
			// "\u000c" == "\f" == form feed.
			'\u0009' => true,
			'\u000c' => true,
			'\u0020' => true,
			'\u1680' => true,
			'\u180e' => true,
			'\u2028' => true,
			'\u205f' => true,
			'\u3000' => true,
			'\u2000' => true,
			'\u2001' => true,
			'\u2002' => true,
			'\u2003' => true,
			'\u2004' => true,
			'\u2005' => true,
			'\u2006' => true,
			'\u2007' => true,
			'\u2008' => true,
			'\u2009' => true,
			'\u200a' => true,
			_ => false
		};
	}

	/// <summary>
	/// Returns whether <paramref name="c"/> is a JRSR linefeed character.
	/// This method returns false for all surrogates.
	/// </summary>
	private static bool IsLinefeed(char c)
	{
		return c switch
		{
			// https://tasvideos.org/EmulatorResources/JPC/JRSRFormat#LineFeeds
			// https://repo.or.cz/jpcrr.git/blob/6ab255ce10b2dd10b0ac3ab2e2be708e0b26eeaa:/org/jpc/jrsr/UTFInputLineStream.java#l194
			'\u000a' => true,
			'\u000d' => true,
			'\u001c' => true,
			'\u001d' => true,
			'\u001e' => true,
			'\u0085' => true,
			'\u2029' => true,
			_ => false
		};
	}

	/// <summary>
	/// Parses ahead in the file to find the beginning of the next section,
	/// skipping over remaining lines in the current section if any, and
	/// returns the section name. Returns <c>null</c> if there are no more
	/// sections.
	/// </summary>
	/// <exception name="FormatException">There is a syntax error in the
	/// file, or a section name exceeds the length limit.</exception>
	public async Task<string?> NextSection()
	{
		while (true)
		{
			// We are now at the beginning of a line (or at end of file).
			var c = await ReadChar();
			if (c == -1)
			{
				if (_inSection)
				{
					throw new FormatException("Expected !END before end of file");
				}
				else
				{
					// EOF and not in a section, we are done.
					return null;
				}
			}
			else if (IsLinefeed((char)c))
			{
				// Blank lines are ignored.
			}
			else if ((char)c == '!')
			{
				// Command is expected to be "BEGIN" or "END". cc is the
				// character that immediately follows the command.
				var command = new StringBuilder();
				int cc;
				while (true)
				{
					cc = await ReadChar();
					if (cc == -1)
					{
						throw new FormatException("Unexpected end of file");
					}
					else if (IsSpace((char)cc) || IsLinefeed((char)cc))
					{
						break;
					}

					// We are only expecting BEGIN or END here. If we read
					// more than 5 characters without finding a space or a
					// linefeed, it's an error.
					if (command.Length >= 5)
					{
						throw new FormatException("Expected BEGIN or END after !");
					}

					command.Append((char)cc);
				}

				if (command.ToString() == "BEGIN")
				{
					// Skip over the spaces that follow !BEGIN. There must
					// be at least one.
					if (!IsSpace((char)cc))
					{
						throw new FormatException("Expected space after !BEGIN");
					}

					while (true)
					{
						cc = await ReadChar();
						if (cc == -1)
						{
							throw new FormatException("Unexpected end of file");
						}
						else if (!IsSpace((char)cc))
						{
							break;
						}
					}

					// The section name is everything to the end of the
					// line, including spaces.
					var sectionName = new StringBuilder();
					while (true)
					{
						if (cc == -1)
						{
							throw new FormatException("Unexpected end of file");
						}
						else if (IsLinefeed((char)cc))
						{
							break;
						}

						if (sectionName.Length >= _lengthLimit)
						{
							throw new FormatException("Section name exceeds length limit");
						}

						sectionName.Append((char)cc);
						cc = await ReadChar();
					}

					if (sectionName.Length == 0)
					{
						throw new FormatException("Expected section name after !BEGIN");
					}

					// We have found the name of the next section, and are
					// ready to start reading the section's lines.
					_inSection = true;
					return sectionName.ToString();
				}
				else if (command.ToString() == "END")
				{
					if (!IsLinefeed((char)cc))
					{
						throw new FormatException("Expected linefeed after !END");
					}

					if (!_inSection)
					{
						throw new FormatException("Expected !BEGIN before !END");
					}

					// The current section is ended (so now '+' at the
					// beginning of a line is a syntax error, not a line to
					// be skipped). We are still looking for the beginning
					// of the next section and its section name.
					_inSection = false;
				}
				else
				{
					throw new FormatException("Expected BEGIN or END after !");
				}
			}
			else if ((char)c == '+' && _inSection)
			{
				// We are looking for the next section, so skip over any
				// lines in the current section.
				while (true)
				{
					var cc = await ReadChar();
					if (cc == -1)
					{
						throw new FormatException("Unexpected end of file");
					}
					else if (IsLinefeed((char)cc))
					{
						break;
					}
				}
			}
			else
			{
				if (!_inSection)
				{
					throw new FormatException("Expected !BEGIN");
				}
				else
				{
					throw new FormatException("Expected !END");
				}
			}
		}
	}

	/// <summary>
	/// Parses ahead in the file to find the next in-section line, and
	/// returns the line. Returns <c>null</c> if there are no more lines.
	/// </summary>
	/// <exception name="FormatException">There is a syntax error in the
	/// file, or the line exceeds the length limit.</exception>
	/// <exception name="InvalidOperationException">The method was called
	/// while not in a section.</exception>
	public async Task<string?> NextLine()
	{
		if (!_inSection)
		{
			throw new InvalidOperationException("Not in a section");
		}

		while (true)
		{
			var c = await ReadChar();
			if (c == -1)
			{
				throw new FormatException("Unexpected end of file");
			}
			else if (IsLinefeed((char)c))
			{
				// Blank lines are ignored.
			}
			else if ((char)c == '+')
			{
				var line = new StringBuilder();
				while (true)
				{
					var cc = await ReadChar();
					if (cc == -1)
					{
						throw new FormatException("Unexpected end of file");
					}
					else if (IsLinefeed((char)cc))
					{
						break;
					}

					if (line.Length >= _lengthLimit)
					{
						throw new FormatException("Line exceeds length limit");
					}

					line.Append((char)cc);
				}

				return line.ToString();
			}
			else
			{
				// Whatever is here, it is not a line. Let NextSection check
				// the syntax of whatever follows.
				UnreadChar((char)c);
				return null;
			}
		}
	}

	/*
	* https://tasvideos.org/EmulatorResources/JPC/JRSRFormat#LineComponentFormat
	* https://repo.or.cz/jpcrr.git/blob/6ab255ce10b2dd10b0ac3ab2e2be708e0b26eeaa:/org/jpc/Misc.java#l232
	*
	* Grammar for "component" format:
	*   component ::= (space* token)* space*
	*   token ::= plain | "(" paren ")"
	*   char  ::= "\" any | [^()\]
	*   plain ::= (char - space)+
	*   paren ::= (char | "(" paren ")")+
	*
	* The documentation says that component format "encodes a non-empty
	* array of non-empty strings," but the parsing algorithm given there
	* actually permits the array to be empty. The individual strings must
	* be non-empty.
	*
	* There are two ways of escaping characters that would otherwise
	* separate tokens: prefixing with a backslash or enclosing in
	* parentheses. Within a parentheses-escaped token, parentheses must be
	* balanced unless backslash-escaped. The outermost pair of parentheses
	* is discarded, but the parentheses inside are retained as part of the
	* token.
	*
	* In the context of a JRSR file, it is impossible for a line to
	* contain a linefeed character, because it would have terminated the
	* line. If we encounter a linefeed, we treat it as a normal token
	* character.
	*/

	/// <summary>
	/// Returns an iterator over the tokens of of a line in "component"
	/// format.
	/// </summary>
	/// <exception name="FormatException">The line has unbalanced
	/// parentheses, or a terminating backslash.</exception>
	public static IEnumerable<string> DecodeComponent(string line)
	{
		var token = new StringBuilder();
		var escaped = false;
		var depth = 0;
		foreach (var c in line)
		{
			if (escaped)
			{
				token.Append(c);
				escaped = false;
			}
			else if (IsSpace(c))
			{
				// There is an omission in the tasvideos.org documentation
				// here, as of Revision 7. Space characters outside
				// parentheses (depth == 0) end a token, but inside
				// parentheses they are part of the token.
				if (depth == 0)
				{
					if (token.Length > 0)
					{
						yield return token.ToString();
						token.Clear();
					}
				}
				else
				{
					token.Append(c);
				}
			}
			else if (c == '(')
			{
				if (depth == 0)
				{
					// This left parenthesis terminates the current token
					// and begins a new token.
					if (token.Length > 0)
					{
						yield return token.ToString();
						token.Clear();
					}
				}
				else
				{
					// This left parenthesis is itself enclosed within
					// parentheses and counts as part of the token.
					token.Append(c);
				}

				depth = checked(depth + 1);
			}
			else if (c == ')')
			{
				if (depth == 0)
				{
					throw new FormatException("Unmatched ')'");
				}
				else if (depth == 1)
				{
					// This right parenthesis terminates the current token,
					// which was begin by a left parenthesis.
					if (token.Length > 0)
					{
						yield return token.ToString();
						token.Clear();
					}
				}
				else
				{
					// This right parenthesis is itself enclosed within
					// parentheses and counts as part of the token.
					token.Append(c);
				}

				depth -= 1;
			}
			else if (c == '\\')
			{
				escaped = true;
			}
			else
			{
				token.Append(c);
			}
		}

		if (escaped)
		{
			throw new FormatException("'\\' at end of line");
		}

		if (depth != 0)
		{
			throw new FormatException("Unmatched '('");
		}

		if (token.Length > 0)
		{
			yield return token.ToString();
			token.Clear();
		}
	}

	/// <summary>
	/// Returns true if and only if <paramref name="eventClass"/> is
	/// non-empty and consists only of characters 'A'-'Z' and '0'-'9'.
	/// </summary>
	public static bool IsSpecialEventClass(string eventClass)
	{
		// https://tasvideos.org/EmulatorResources/JPC/JRSRFormat#EventsSection
		// "If $class consists only of 'A-Z' and '0-9' (capital letters and
		// numbers) then it is either special event or reserved (error)."
		return eventClass != string.Empty && eventClass.All(c =>
			c is >= 'A' and <= 'Z' or >= '0' and <= '9');
	}
}
