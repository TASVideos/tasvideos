document.addEventListener("DOMContentLoaded", findSubmitButtons);

function findSubmitButtons() {
	Array.from(document.querySelectorAll('button[type="submit"]')).forEach(btn => {
		btn.onclick = function() {
			let btn = this;
			setTimeout(function () { btn.disabled = true }, 0);
			setTimeout(function () { btn.disabled = false }, 750);
		}
	});
}