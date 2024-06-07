let forumHeader = document.getElementById("forum-header");
let forumHeaderRestore = document.getElementById("forum-header-restore");
let dismissBtn = document.getElementById("forum-header-dismiss");

const dismiss = localStorage.getItem("DismissForumHeader");
if (dismiss !== "true") {
	forumHeader.classList.remove("d-none");
	forumHeaderRestore.classList.add("d-none");
} else {
	forumHeaderRestore.classList.remove("d-none");
}

dismissBtn.onclick = function () {
	localStorage.setItem("DismissForumHeader", true);
	forumHeader.classList.add("d-none");
	forumHeaderRestore.classList.remove("d-none");
}

forumHeaderRestore.onclick = function () {
	localStorage.removeItem('DismissForumHeader');
	forumHeaderRestore.classList.add("d-none");
	forumHeader.classList.remove("d-none");
}