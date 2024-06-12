const id = document.getElementById('Id').value;
const obsoletionModel = document.querySelector('[data-id="obsolete"]');
const descriptionModel = document.querySelector('[data-id="wiki-edit"]');
let originalDescription = descriptionModel.value;

descriptionModel.addEventListener('change', function () {
	originalDescription = descriptionModel.value;
});

obsoletionModel.addEventListener('change', function () {
	if (!obsoletionModel.value) {
		descriptionModel.value = originalDescription;
		return;
	}

	document.getElementById("obsoleted-by").innerHTML = "";
	const url = `/Submissions/Publish/${id}?handler=ObsoletePublication&publicationId=${obsoletionModel.value}`;
	fetch(url)
		.then(handleFetchErrors)
		.then(r => r.json())
		.then(t => {
			const oldValue = originalDescription;
			descriptionModel.value = t.markup;
			originalDescription = oldValue;

			document.getElementById("obsoleted-by").innerHTML = t.title ? t.title : "Unknown publication";

			for (let option of document.querySelectorAll('[data-id="tags"] option')) {
				option.selected = t.tags.includes(Number(option.value));
			}

			const tagElem = document.querySelector('[data-id="tags"]');
			engageSelectImprover(tagElem.id);
		});
});