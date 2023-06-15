function highlight(text) {
	const r = /\[\/?[a-z*]+\]/g;
	const results = [];
	let from = 0;
	let match;
	while (match = r.exec(text)) {
		if (from < match.index) {
			results.push(text.slice(from, match.index));
		}
		const span = document.createElement("span");
		span.style.color = "blue";
		span.textContent = match[0];
		results.push(span);
		from = match.index + match[0].length;
	}
	results.push(text.slice(from));
	return results;
}


(() => {
	const textarea = document.querySelector("textarea[data-tempfixme]");
	if (!textarea) {
		return;
	}
	textarea.style.color = "white"; // Light mode only
	const parent = textarea.parentElement;
	parent.style.position = "relative";

	const { paddingLeft, paddingRight, paddingTop, paddingBottom, } = getComputedStyle(textarea)

	const overlay = document.createElement("div");
	Object.assign(overlay.style, {
		position: "absolute",
		inset: 0,
		pointerEvents: "none",
		paddingLeft,
		paddingRight,
		paddingTop,
		paddingBottom,
		border: "1px solid #0000",
		whiteSpace: "pre-wrap",
		overflowY: "auto",
	});

	parent.appendChild(overlay);

	textarea.addEventListener("input", () => {
		overlay.textContent = "";
		overlay.append(...highlight(textarea.value));
	}, { passive: true });
	textarea.addEventListener("scroll", () => {
		overlay.scrollTop = textarea.scrollTop;
	}, { passive: true });
})();
