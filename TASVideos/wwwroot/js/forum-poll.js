window.addEventListener("DOMContentLoaded", () => {
	Array.from(document.querySelectorAll('[name="ordinal"]')).forEach(element => {
		element.addEventListener('change', () => {
			document.getElementById('vote-btn').removeAttribute('hidden');
		});
	});
});

