{
	let feedHeader = document.querySelector("[data-id='feed-header']");
	let dismissBtn = document.querySelector("[data-id='feed-header-dismiss']");

	const dismiss = localStorage.getItem("DismissFeedHeader");
	if (dismiss !== "true" && !window.location.pathname.startsWith("/Feed")) {
		feedHeader.classList.remove("d-none");
	}
	if (window.location.pathname === "/") {
		feedHeader.classList.remove("d-none");
		dismissBtn.classList.add("d-none");
		feedHeader.classList.replace("mb-2", "mb-0")
	}

	dismissBtn.onclick = function () {
		localStorage.setItem("DismissFeedHeader", true);
		feedHeader.classList.add("d-none");
	}
}