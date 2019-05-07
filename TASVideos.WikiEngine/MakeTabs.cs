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
			var navClass = tabset.Tag == "htabs" ? "nav nav-pills nav-stacked col-md-3" : "nav nav-tabs";
			var liClass = tabset.Tag == "htabs" ? "" : "nav-item";
			var aClass = tabset.Tag == "htabs" ? "" : "nav-link";
			var tabClass = tabset.Tag == "htabs" ? "tab-content col-md-9" : "tab-content";
			var nav = new List<INode>();
			var content = new List<INode>();
			var first = true;

			var index = 0;
			foreach (var child in tabset.Children.Cast<Element>())
			{
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
							new[] { Attr("href", "#" + id), Attr("data-toggle", "tab"), Attr("class", (first ? "active " : "") + aClass) },
							new[]
							{
								new Text(child.CharStart, child.Attributes["data-name"])
							})
					}));
				content.Add(new Element(child.CharStart, "div", new[] { Attr("id", id), Attr("class", "tab-pane" + (first ? " active" : " fade")) }, child.Children));
				first = false;
			}

			return new Element(tabset.CharStart, "div", new[] { Attr("class", "") }, new[]
			{
				new Element(tabset.CharStart, "ul", new[] { Attr("class", navClass), Attr("role", "tablist") }, nav),
				new Element(tabset.CharStart, "div", new[] { Attr("class", tabClass) }, content)
			});
		}
	}
}
