document.addEventListener("DOMContentLoaded", findWikiEditBox);

function findWikiEditBox() {
	const textArea = document.querySelector('[data-id="wiki-edit"]');
	wireUpWikiEditHelper(textArea);
}
function wireUpWikiEditHelper(textArea) {
	const buttons = document.querySelectorAll("button[data-fmt]");

	function applyFormatting(s) {
		const ss = s.split(",");
		let val = textArea.value;
		const i = textArea.selectionStart;
		const j = textArea.selectionEnd;
		if (val.slice(i - ss[0].length, i) === ss[0] && val.slice(j, j + ss[1].length) === ss[1]) {
			val = val.slice(0, i - ss[0].length) + val.slice(i, j) + val.slice(j + ss[1].length);
			textArea.value = val;
			textArea.selectionStart = i - ss[0].length;
			textArea.selectionEnd = j - ss[0].length;
		} else {
			val = val.slice(0, i) + ss[0] + val.slice(i, j) + ss[1] + val.slice(j);
			textArea.value = val;
			textArea.selectionStart = i + ss[0].length;
			textArea.selectionEnd = j + ss[0].length;
		}

		textArea.dispatchEvent(new Event('change'));
	}

	for (let i = 0; i < buttons.length; i++) {
		buttons[i].addEventListener("click", function () {
			applyFormatting(this.getAttribute("data-fmt"));
			textArea.focus();
		});
	}

	textArea.addEventListener("keydown", function listener(ev) {
		let button;
		if (ev.altKey) {
			button = document.querySelector(`button[data-fmt][data-akey='${ev.key}']`);
		} else if (ev.ctrlKey && ev.shiftKey) {
			button = document.querySelector(`button[data-fmt][data-skey='${ev.key}']`);
		} else if (ev.ctrlKey) {
			button = document.querySelector(`button[data-fmt][data-key='${ev.key}']`);
		}

		if (button) {
			ev.preventDefault();
			applyFormatting(button.getAttribute("data-fmt"));
		}
	});
}