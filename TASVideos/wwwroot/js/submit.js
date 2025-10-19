// Client side validation for .zip, consider removing this at some point
jQuery.validator.addMethod('nozip', function (value, element) {
    if (!value) {
        return true; // Let the required validator handle empty values
    }

    const extension = value.split('.').pop().toLowerCase();
    return extension !== 'zip';
}, 'ZIP files are not supported. Please upload the original movie file.');

jQuery(document).ready(function () {
    const movieFileInput = document.querySelector('[data-id="movie-file"]');
    if (movieFileInput) {
        jQuery(movieFileInput).rules('add', {
            nozip: true
        });
    }
});

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
const submitBtn = document.getElementById('submit-btn');
if (markupBox.value.length === 0) {
	submitBtn.classList.add('d-none');
}
document.getElementById('preview-button').addEventListener('click', function () {
	document.getElementById('submit-btn').classList.remove('d-none');
});