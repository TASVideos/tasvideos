window.addEventListener("load", function() {
	const rightMarkupElem = document.querySelector('[data-id=wiki-markup]');
	rightMarkupElem.addEventListener("change", generateDiff);
	rightMarkupElem.addEventListener("keyup", generateDiff);
});