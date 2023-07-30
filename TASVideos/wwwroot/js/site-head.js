function forceDarkMode() {
	removeAutoDarkMode();
	document.getElementById("style-dark").disabled = false;

	localStorage.setItem("style-dark", "true");
}

function forceLightMode() {
	removeForcedDarkMode();
	removeAutoDarkMode();

	localStorage.setItem("style-dark", "false");
}

function autoDarkMode() {
	removeForcedDarkMode();
	document.getElementById("style-dark-initial").disabled = false;

	localStorage.removeItem("style-dark");
}

function removeForcedDarkMode() {
	document.getElementById("style-dark").disabled = true;
}

function removeAutoDarkMode() {
	document.getElementById("style-dark-initial").disabled = true;
}

if (localStorage.getItem("style-dark") !== null) {
	removeAutoDarkMode();
	if (localStorage.getItem("style-dark") === "true") {
		forceDarkMode();
	}
}
