function renderDiff(from, to, destEl, inline, contextSize) {
	const dmp = new diff_match_patch();
	dmp.Diff_Timeout = 0;
	dmp.Diff_EditCost = 20;

	const d = dmp.diff_main(from.text, to.text);
	dmp.diff_cleanupEfficiency(d);

	const allLines = [];

	{
		let leftLineNumber = 1;
		let rightLineNumber = 1;
		let spans = [];
		function flush() {
			allLines.push({
				leftLineNumber,
				rightLineNumber,
				spans,
				important: spans.some(s => s.type),
				distance: 99999999,
			});
		}
		for (const { 0: type, 1: text } of d) {
			const textParts = text.split("\n");
			for (let i = 0; i < textParts.length; i++) {
				spans.push({ type, text: textParts[i] });
				if (i < textParts.length - 1) {
					flush();
					spans = [];
					if (type <= 0) { leftLineNumber++; }
					if (type >= 0) { rightLineNumber++; }
				}
			}
		}
		if (spans.length) {
			flush();
		}
	}

	{
		for (let i = 0; i < allLines.length; i++) {
			const line = allLines[i];
			if (line.important) {
				line.distance = 0;
			} else if (i > 0) {
				line.distance = allLines[i - 1].distance + 1;
			}
		}
		for (let i = allLines.length - 1; i >= 0; i--) {
			const line = allLines[i];
			if (!line.important && i < allLines.length - 1) {
				line.distance = Math.min(line.distance, allLines[i + 1].distance + 1);
			}
		}
	}

	const condensedLines = [];

	{
		let condensedSet = [];
		function flush() {
			if (condensedSet.length) {
				condensedLines.push(condensedSet);
				condensedSet = [];
			}
		}
		for (const line of allLines) {
			if (line.distance <= contextSize) {
				flush();
				condensedLines.push(line);
			} else {
				condensedSet.push(line);
			}
		}
		flush();
	}

	function h(tag, ...args) {
		const e = document.createElement(tag);
		function processChildren(children) {
			for (const child of children) {
				if (typeof child === "string" || typeof child === "number") {
					e.appendChild(new Text(child));
				} else if (Array.isArray(child)) {
					processChildren(child);
				} else if (child instanceof Element) {
					e.appendChild(child);
				} else {
					for (const k in child) {
						e.setAttribute(k, child[k]);
					}
				}
			}
		}
		processChildren(args);
		return e;
	}

	const results = [];
	if (inline) {
		results.push(h("div"), h("div"), h("div", { class: "header" }, from.name + "⇒" + to.name));
	} else {
		results.push(
			h("div"),
			h("div", { class: "header" }, from.name),
			h("div"),
			h("div", { class: "header" }, to.name),
		);
	}

	{
		let leftNumber;
		let rightNumber;

		function pushLine(line, classSuffix) {
			function pushSpans(filter) {
				const spans = line.spans.filter(filter)
					.map(({ type, text }) => h("span", { class: type > 0 ? "add" : type < 0 ? "delete" : "keep" }, text));
				if (spans.length) {
					results.push(h("div", { class: "line" + classSuffix }, spans));
				} else {
					results.push(h("div"));
				}
			}

			if (leftNumber !== line.leftLineNumber) {
				results.push(h("div", { class: "linenumber" + classSuffix }, leftNumber = line.leftLineNumber));
			} else {
				results.push(h("div"));
			}
			if (!inline) {
				pushSpans(span => span.type <= 0);
			}
			if (rightNumber !== line.rightLineNumber) {
				results.push(h("div", { class: "linenumber" + classSuffix }, rightNumber = line.rightLineNumber));
			} else {
				results.push(h("div"));
			}
			pushSpans(span => inline || span.type >= 0);
		}

		for (let i = 0; i < condensedLines.length; i++) {
			const lineOrSet = condensedLines[i];
	
			if (Array.isArray(lineOrSet)) {
				results.push(h("button", { class: "expander", type: "button" }, "⋯"));
				results.push(h("button", { class: "contracter top", type: "button" }, "▼"));
				leftNumber = undefined;
				rightNumber = undefined;
				for (const line of lineOrSet) {
					pushLine(line, " expanded");
				}
				results.push(h("button", { class: "contracter bottom", type: "button" }, "▲"))
				leftNumber = undefined;
				rightNumber = undefined;
			} else {
				pushLine(lineOrSet, "");
			}
		}
	}

	destEl.innerHTML = "";
	destEl.appendChild(h("div", { class: inline ? "diff inline" : "diff sidebyside" }, results));
	destEl.children[0].addEventListener("click", (event) => {
		const { target } = event;
		const { classList } = target;
		if (classList.contains("expander")) {
			target.style.display = "none";
			let element = target;
			while (!element.classList.contains("bottom")) {
				element = element.nextElementSibling;
				element.style.display = "unset";
			}
		} else if (classList.contains("contracter")) {
			target.style.display = "";
			if (classList.contains("top")) {
				target.previousElementSibling.style.display = "";
				let element = target;
				while (!element.classList.contains("bottom")) {
					element = element.nextElementSibling;
					element.style.display = "";
				}
			} else {
				let element = target;
				while (!element.classList.contains("expander")) {
					element = element.previousElementSibling;
					element.style.display = "";
				}
			}
		}
	}, { passive: true });
}
