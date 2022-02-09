function showHideScrollToTop() {
	if (window.scrollY > 20) {
		document.getElementById("button-scrolltop").classList.remove("d-none");
	} else {
		document.getElementById("button-scrolltop").classList.add("d-none");
	}
}

function scrollToTop() {
	window.scroll({
		top: 0,
		behavior: 'smooth'
	});
}

window.addEventListener("scroll", showHideScrollToTop);
document.getElementById("button-scrolltop").addEventListener("click", scrollToTop);

if (location.hash) {
	let expandButton = document.querySelector(`[data-bs-target='${location.hash}']`);
	if (expandButton) {
		expandButton.classList.remove("collapsed");
		expandButton.setAttribute("aria-expanded", "true");
	}
	if (isNaN(parseInt(location.hash.substr(1)))) {
		let expandedContent = document.querySelector(location.hash + '.collapse');
		if (expandedContent) {
			expandedContent.classList.add("show");
		}
	}
}
