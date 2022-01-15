using System.Collections.Generic;
using System.Linq;
using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine
{
	public static partial class Builtins
	{
		public static INode MakeTabs(Element tabset)
		{
			// TODO: Fix up CharEnds
			var parentDivClass = tabset.Tag == "htabs" ? "row" : "";
			var navDivClass = tabset.Tag == "htabs" ? "col-md-3" : "";
			var navClass = tabset.Tag == "htabs" ? "nav nav-pills nav-stacked" : "nav nav-tabs";
			var liClass = "nav-item";
			var aClass = "nav-link";
			var tabClass = tabset.Tag == "htabs" ? "tab-content col-md-9" : "tab-content";
			var nav = new List<INode>();
			var content = new List<INode>();
			var first = true;

			var index = 0;
			foreach (var child in tabset.Children.Cast<Element>())
			{
				if (child.Tag != "tab")
				{
					throw new NewParser.SyntaxException("Non-tab content inside a tabset", child.CharStart);
				}
				var id = "tabs-" + tabset.CharStart + "-" + index++;
				nav.Add(new Element(
					child.CharStart,
					"li",
					new[]
					{
						Attr("class",  liClass)
					},
					new[]
					{
						new Element(
							child.CharStart,
							"a",
							new[] { Attr("href", "#" + id), Attr("data-bs-toggle", "tab"), Attr("class", (first ? "active " : "") + aClass) },
							new[]
							{
								new Text(child.CharStart, child.Attributes["data-name"])
							})
					}));
				content.Add(new Element(child.CharStart, "div", new[] { Attr("id", id), Attr("class", "tab-pane" + (first ? " active" : " fade")) }, child.Children));
				first = false;
			}

			return new Element(tabset.CharStart, "div", new[] { Attr("class", parentDivClass) }, new[]
			{
				new Element(tabset.CharStart, "div", new[] { Attr("class", navDivClass) }, new[]
				{
					new Element(tabset.CharStart, "ul", new[] { Attr("class", navClass), Attr("role", "tablist") }, nav)
				}),
				new Element(tabset.CharStart, "div", new[] { Attr("class", tabClass) }, content)
			});
		}
	}
}
