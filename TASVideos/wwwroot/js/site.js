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
	})
}

window.addEventListener("scroll", showHideScrollToTop);
document.getElementById("button-scrolltop").addEventListener("click", scrollToTop);

if (location.hash) {
	let expandButton = document.querySelector(`[href='${location.hash}']`);
	if (expandButton) {
		expandButton.setAttribute("aria-expanded", "true");
	}
	let expandedContent = document.querySelector(location.hash + '.collapse');
	if (expandedContent) {
		expandedContent.classList.add("show");
	}
}
