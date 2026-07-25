using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine;

public static partial class Builtins
{
	public static INode PromoteNavItem(Element imgElem)
		=> new Element(
			imgElem.CharStart,
			"a",
			[
				Attr("class", "dropdown-item"),
				Attr("href", imgElem.Attributes["src"]),
			],
			[
				new Text(imgElem.CharStart, imgElem.Attributes["title"]),
			]);
}
