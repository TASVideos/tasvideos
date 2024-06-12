window.addEventListener('load', validateForm);
const classes = document.querySelector('[data-id="classes"]');
const years = document.querySelector('[data-id="years"]');
const genres = document.querySelector('[data-id="genres"]');
const flags = document.querySelector('[data-id="flags"]');
const gameGroups = document.querySelector('[data-id="game-groups"]');
const authors = document.querySelector('[data-id="authors"]');
const systemCodes = document.querySelector('[data-id="systems"]');
const tags = document.querySelector('[data-id="tags"]');

classes.onchange = validateForm;
years.onchange = validateForm;
genres.onchange = validateForm;
flags.onchange = validateForm;
gameGroups.onchange = validateForm;
authors.onchange = validateForm;
systemCodes.onchange = validateForm;
tags.onchange = validateForm;

function validateForm() {
	document.getElementById('filter-btn').disabled = !anySelected();
}

function anySelected() {
	return classes.selectedIndex >= 0
		|| years.selectedIndex >= 0
		|| genres.selectedIndex >= 0
		|| flags.selectedIndex >= 0
		|| gameGroups.selectedIndex >= 0
		|| authors.selectedIndex >= 0
		|| systemCodes.selectedIndex >= 0
		|| tags.selectedIndex >= 0;
}