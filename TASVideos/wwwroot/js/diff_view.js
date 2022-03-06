function renderDiff(from, to, el, inline) {
	const dmp = new diff_match_patch();
	dmp.Diff_Timeout = 0;
	dmp.Diff_EditCost = 20;

	const d = dmp.diff_main(from.text, to.text);
	dmp.diff_cleanupEfficiency(d);

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

	class LineCounter {
		number = 0;
		renderedNumber = 0;
		start() {
			if (this.number === 0) {
				this.increment();
			}
		}
		increment() { this.number++; }
		render() {
			if (this.renderedNumber !== this.number) {
				this.renderedNumber = this.number;
				return h("div", { class: "linenumber" }, this.number);
			} else {
				return h("div");
			}
		}
	}

	if (inline) {
		let oldl = new LineCounter;
		let newl = new LineCounter;
		const line = [];
		function flush() {
			results.push(oldl.render(), newl.render(), h("div", { class: "line" }, line));
			line.length = 0;
		}

		for (const { 0: type, 1: text } of d) {
			const clazz = type > 0 ? "add" : type < 0 ? "delete" : "keep";
			const parts = text.split("\n");
			if (type <= 0) { oldl.start(); }
			if (type >= 0) { newl.start(); }
			let first = true;
			for (const part of parts) {
				if (first) {
					first = false;
				} else {
					line.push("\n");
					flush();
					if (type <= 0) { oldl.increment(); }
					if (type >= 0) { newl.increment(); }
				}
				line.push(h("span", { class: clazz }, part));
			}
		}
		if (line.length) {
			flush();
		}
	} else {
		class SideBySideLine {
			counter = new LineCounter;
			line = [];
			text(text, clazz) {
				this.counter.start();
				this.line.push(h("span", { class: clazz }, text));
			}
			flush() {
				if (this.line.length) {
					const ret = [
						this.counter.render(),
						h("div", { class: "line" }, this.line),
					];
					this.line.length = 0;
					return ret;
				} else {
					return [
						h("div"),
						h("div"),
					];
				}
			}
		}
		const left = new SideBySideLine;
		const right = new SideBySideLine;
		
		function flush() {
			results.push(...left.flush(), ...right.flush());
		}

		for (const { 0: type, 1: text } of d) {
			const clazz = type > 0 ? "add" : type < 0 ? "delete" : "keep";
			const parts = text.split("\n");
			let first = true;
			for (const part of parts) {
				if (first) {
					first = false;
				} else {
					flush();
					if (type <= 0) { left.counter.increment(); }
					if (type >= 0) { right.counter.increment(); }
				}
				if (type <= 0) { left.text(part, clazz); }
				if (type >= 0) { right.text(part, clazz); }
			}
		}
		if (left.line.length || right.line.length) {
			flush();
		}
	}

	el.innerHTML = "";
	el.appendChild(h("div", { class: inline ? "diff inline" : "diff sidebyside" }, results));
}
