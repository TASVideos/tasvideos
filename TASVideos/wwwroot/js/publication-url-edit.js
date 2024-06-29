window.addEventListener("DOMContentLoaded", function () {
	const urlBox = document.querySelector('[data-id="url"]');
	urlBox.addEventListener('input', () => {
		if (urlBox.value && urlBox.value.includes('archive.org')) {
			const typeDropdown = document.querySelector('[data-id="url-types"]');
			typeDropdown.value = '1';
		}
	});
});