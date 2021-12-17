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

function forceDarkMode() {
	let DarkModeStylesheet = document.getElementById("style-dark");
	if (!DarkModeStylesheet) {
		removeAutoDarkMode();

		var newElement = document.createElement('link');
		newElement.rel = "stylesheet";
		newElement.id = "style-dark";
		newElement.href = "/css/darkmode.css";
		document.head.appendChild(newElement);

		localStorage.setItem("style-dark", "true");
	}
}

function forceLightMode() {
	removeForcedDarkMode();
	removeAutoDarkMode();

	localStorage.setItem("style-dark", "false");
}

function autoDarkMode() {
	let initialDarkModeStylesheet = document.getElementById("style-dark-initial");
	if (!initialDarkModeStylesheet) {
		removeForcedDarkMode();

		var newElement = document.createElement('link');
		newElement.rel = "stylesheet";
		newElement.id = "style-dark-initial";
		newElement.href = "/css/darkmode-initial.css";
		document.head.appendChild(newElement);

		localStorage.removeItem("style-dark");
	}
}

function removeForcedDarkMode() {
	let DarkModeStylesheet = document.getElementById("style-dark");
	if (DarkModeStylesheet) {
		DarkModeStylesheet.parentElement.removeChild(DarkModeStylesheet);
	}
}

function removeAutoDarkMode () {
	let initialDarkModeStylesheet = document.getElementById("style-dark-initial");
	if (initialDarkModeStylesheet) {
		initialDarkModeStylesheet.parentElement.removeChild(initialDarkModeStylesheet);
	}
}

if (localStorage.getItem("style-dark") !== null) {
	removeAutoDarkMode();
	if (localStorage.getItem("style-dark") === "true") {
		forceDarkMode();
	}
}

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
