function disableSubmit() {
	const btn = document.getElementById("submit-btn");
	btn.classList.add("disabled");
	btn.disabled = true;
	btn.setAttribute("aria-disabled", true);
}

function enableSubmit() {
	const btn = document.getElementById("submit-btn");
	btn.classList.remove("disabled");
	btn.disabled = false;
	btn.removeAttribute("aria-disabled");
}

function ratingConnect(checkbox, textbox, slider) {
	checkbox.onchange = function () {
		if (checkbox.checked) {
			textbox.value = '';
		}
	};
	textbox.oninput = function () {
		slider.value = textbox.value;
		checkbox.checked = false;
		enableSubmit();
	};
	slider.oninput = function () {
		slider.value = Math.round(Number(this.value) * 2) / 2;
		textbox.value = slider.value;
		checkbox.checked = false;
		enableSubmit();
	};
}

ratingConnect(document.getElementById('unrated'), document.getElementById('Rating'), document.getElementById('entertainmentSlider'));