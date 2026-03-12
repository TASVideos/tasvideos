const unsecureLinkWarning = document.getElementById("unsecure-link-warning");
document.getElementById("Markup").addEventListener("change", event => {
	if (event.target.value.includes("http://")) unsecureLinkWarning.classList.remove("d-none");
	else unsecureLinkWarning.classList.add("d-none");
});
