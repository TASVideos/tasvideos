﻿using TASVideos.WikiEngine.AST;

namespace TASVideos.WikiEngine;

public static partial class Builtins
{
	public static INode MakeTabs(Element tabset)
	{
		const string liClass = "nav-item";
		const string aClass = "nav-link";

		// TODO: Fix up CharEnds
		var parentDivClass = tabset.Tag == "htabs" ? "row" : "";
		var navDivClass = tabset.Tag == "htabs" ? "col-md-3" : "";
		var navClass = tabset.Tag == "htabs" ? "nav nav-pills nav-stacked" : "nav nav-tabs";
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
				[
					Attr("class", liClass)
				],
				new[]
				{
						new Element(
							child.CharStart,
							"a",
							[Attr("href", "#" + id), Attr("data-bs-toggle", "tab"), Attr("class", (first ? "active " : "") + aClass)],
							new[]
							{
								new Text(child.CharStart, child.Attributes["data-name"])
							})
				}));
			content.Add(new Element(child.CharStart, "div", [Attr("id", id), Attr("class", "tab-pane fade" + (first ? " active show" : ""))], child.Children));
			first = false;
		}

		return new Element(tabset.CharStart, "div", [Attr("class", parentDivClass)], new[]
		{
			new Element(tabset.CharStart, "div", [Attr("class", navDivClass)], new[]
			{
				new Element(tabset.CharStart, "ul", [Attr("class", navClass), Attr("role", "tablist")], nav)
			}),
			new Element(tabset.CharStart, "div", [Attr("class", tabClass)], content)
		});
	}
}
