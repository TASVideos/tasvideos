const onlyWikiCheckbox = document.getElementById('only-wiki-checkbox');
onlyWikiCheckbox.addEventListener('click', () => {
	let hide = false;
	if (onlyWikiCheckbox.checked) {
		hide = true;
	}

	Array.from(document.querySelectorAll('.developed')).forEach(e => {
		if (hide) {
			e.classList.add('d-none');
		} else {
			e.classList.remove('d-none');
		}
	});
});