let welcomeHeader = document.getElementById("welcome-header");
let dismissBtn = document.getElementById("welcome-header-dismiss");
const dismiss = localStorage.getItem("DismissWelcomeHeader");
if (dismiss !== "true") {
	welcomeHeader.classList.remove("d-none");
}

dismissBtn.onclick = function () {
	localStorage.setItem("DismissWelcomeHeader", true);
	welcomeHeader.classList.add("d-none");
}