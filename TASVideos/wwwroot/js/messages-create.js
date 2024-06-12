const userFormGroup = document.getElementById('user-form-group');
const userBox = document.querySelector('[data-id="to-user"]');
const formSubmitBtn = userBox
	.closest('form')
	.querySelector("button[type='Submit']");

if (document.getElementById('quote-btn')) {
	document.getElementById('quote-btn').onclick = function () {
		const text = document.getElementById('replying-to-text').innerHTML;
		document.querySelector('[data-id="forum-edit"]').innerHTML = `[quote]${text}[/quote]`;
	};
}

const groupSelect = document.getElementById('group-select');
groupSelect.addEventListener('change', () => {
	if (groupSelect.value) {
		userBox.value = groupSelect.value;
		formSubmitBtn.removeAttribute('disabled');

		userFormGroup.classList.add('d-none');
	} else {
		userBox.value = '';
		userFormGroup.classList.remove('d-none');
		formSubmitBtn.setAttribute('disabled', 'disabled');
	}
});