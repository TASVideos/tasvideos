// Autofill timezone if not set
if (!document.querySelector("#TimeZone option:checked").value) {
	const timezoneOffset = 0 - new Date().getTimezoneOffset();
	let timezone = document.querySelector(`[data-offset="${timezoneOffset}"]`);
	timezone.setAttribute('selected', 'selected');
}

function onSubmit() {
	document.getElementById("register-form").submit();
}