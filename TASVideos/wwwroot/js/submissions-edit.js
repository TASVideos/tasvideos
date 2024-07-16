document.querySelector('[data-id="status"]').addEventListener("change", function () {
	const rejectionReasonElem = document.getElementById('rejection-reason');
	const reject = rejectionReasonElem.dataset.rejectionId;
	if (this.value === reject) {
		rejectionReasonElem.classList.remove('d-none');
	} else {
		rejectionReasonElem.classList.add('d-none');
	}
});

document.querySelector('[data-id="wiki-edit"]').addEventListener('change', () => {
	document.getElementById('markup-changed').value = true;
});