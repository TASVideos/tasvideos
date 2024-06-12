const titleSpan = document.getElementById("obsoleted-by");
const id = document.getElementById('Id').value;
const obsoletedByBox = document.querySelector('[data-id="obsoleted-by"]');
obsoletedByBox.onchange = function () {
	const url = `/Publications/Edit/${id}?handler=Title&publicationId=${this.value}`;
	fetch(url)
		.then(handleFetchErrors)
		.then(r => r.text())
		.then(r => {
			titleSpan.innerHTML = r ? r : "Unknown publication Id";
		});
}