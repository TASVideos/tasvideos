namespace TASVideos.ForumEngine.Benchmarks;

public static class TestPosts
{
	public const string BasicFormatting = """
		[b]Bold text[/b] and [i]italic text[/i] with [u]underlined[/u] and [s]strikethrough[/s] formatting.
		
		[center]This is centered text[/center]
		[left]This is left-aligned text[/left]
		[right]This is right-aligned text[/right]
		
		[quote="TestUser"]This is a quote from TestUser with [highlight]highlighted[/highlight] content.[/quote]
		
		[warning]This is a warning message![/warning]
		[note]This is an informational note.[/note]
		
		[spoiler]This is spoiler content that should be hidden.[/spoiler]
		
		Text formatting: [color=red]red text[/color], [bgcolor=yellow]yellow background[/bgcolor], [size=14]larger text[/size]
		
		Chemical formulas: H[sub]2[/sub]O and E=mc[sup]2[/sup]
		
		[tt]Monospace/typewriter text[/tt]
		
		Links and references:
		- Direct URL: https://tasvideos.org
		- [url=https://tasvideos.org]TASVideos Homepage[/url]
		- [url]https://example.com[/url]
		- [email=test@example.com]Send Email[/email]
		- [email]contact@tasvideos.org[/email]
		
		TASVideos-specific links:
		- [movie=1000]Movie #1000[/movie]
		- [submission=5000]Submission #5000[/submission]
		- [game=123]Game #123[/game]
		- [gamegroup=45]Game Group #45[/gamegroup]
		- [thread=12345]Thread #12345[/thread]
		- [post=67890]Post #67890[/post]
		- [userfile=999]User File #999[/userfile]
		- [wiki=GameResources]Game Resources Wiki[/wiki]
		
		Timing information:
		- [frames]3600[/frames] frames at default 60fps
		- [frames]7200@50.0[/frames] frames at 50fps
		
		[google]TAS speedrun[/google] or [google=images]Mario speedrun[/google]
		
		[code=cpp]
		#include <iostream>
		int main() {
			std::cout << "Hello TAS World!" << std::endl;
			return 0;
		}
		[/code]
		
		[code=main.cpp]
		// Download link example
		int x = 42;
		[/code]
		
		[img=640x480]https://example.com/screenshot.png[/img]
		[img]https://example.com/image.jpg[/img]
		
		[video=800x600]https://www.youtube.com/watch?v=dQw4w9WgXcQ[/video]
		
		Lists:
		[list]
		[*]First item with [b]bold[/b] text
		[*]Second item with [url=https://example.com]a link[/url]
		[*]Third item with [code]inline code[/code]
		[/list]
		
		Ordered list:
		[list=1]
		[*]Step one
		[*]Step two with [i]emphasis[/i]
		[*]Final step
		[/list]
		
		[table]
		[tr][th]Header 1[/th][th]Header 2[/th][th]Header 3[/th][/tr]
		[tr][td]Cell A1[/td][td][b]Bold B1[/b][/td][td]C1[/td][/tr]
		[tr][td]A2[/td][td]B2 with [url=https://tasvideos.org]link[/url][/td][td]C2[/td][/tr]
		[/table]
		
		[hr]
		
		[noparse]This [b]won't[/b] be [i]parsed[/i] as markup.[/noparse]
		
		HTML tags (if enabled):
		<b>HTML bold</b>, <i>HTML italic</i>, <em>HTML emphasis</em>
		<u>HTML underline</u>, <pre>HTML preformatted</pre>
		<code>HTML code</code>, <tt>HTML teletype</tt>
		<strike>HTML strikethrough</strike>, <s>HTML strikethrough short</s>, <del>HTML delete</del>
		<sup>HTML superscript</sup>, <sub>HTML subscript</sub>
		<div>HTML div block</div>, <small>HTML small text</small>
		Line break:<br>After break
		Horizontal rule:<hr>After rule
		""";
}
