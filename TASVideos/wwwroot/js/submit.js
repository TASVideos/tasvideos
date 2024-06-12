const markupBox = document.querySelector('[data-id="wiki-edit"]');
document.getElementById('prefill-btn').onclick = function () {
	const markup = markupBox.value;
	if (markup) {
		return;
	}

	fetch("/Submissions/Submit?handler=PrefillText")
		.then(handleFetchErrors)
		.then(r => r.json())
		.then(data => {
			markupBox.value = data.text;
		});
};
document.getElementById('preview-button').addEventListener('click', function () {
	document.getElementById('submit-btn').classList.remove('d-none');
});