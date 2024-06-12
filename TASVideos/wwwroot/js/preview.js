document.getElementById('preview-button').addEventListener('click', function () {
	const previewContainer = document.getElementById('preview-container');
	const previewPath = previewContainer.dataset.path;
	const targetElem = document.querySelector('[data-id="wiki-edit"]')
		?? document.querySelector('[data-id="forum-edit"]')
	const markup = targetElem.value;
	previewContainer.classList.remove('d-none');

	fetch(previewPath, { method: 'POST', body: markup })
		.then(r => {
			if (!r.ok) {
				alert("Could not generate preview");
				throw Error(r.statusText);
			}

			return r;
		})
		.then(r => r.text())
		.then(d => {
			document.getElementById('preview-contents').innerHTML = d;
			Prism.highlightAll();
		})
		.catch(e => alert("Could not generate preview"));
});