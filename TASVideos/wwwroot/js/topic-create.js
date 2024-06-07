const pollContainer = document.getElementById('poll-container');
document.getElementById('add-poll-btn').onclick = function () {
	document.getElementById('add-poll-btn').classList.add('d-none');
	pollContainer.classList.remove('d-none');
	Array.from(pollContainer.querySelectorAll('input'))
		.forEach(element => element.disabled = false);
}

document.getElementById('poll-close').onclick = function () {
	document.getElementById('add-poll-btn').classList.remove('d-none');
	pollContainer.classList.add('d-none');
	Array.from(pollContainer.querySelectorAll('input'))
		.forEach(element => element.disabled = true);
}