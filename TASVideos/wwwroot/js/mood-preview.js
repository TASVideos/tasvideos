const moodImg = document.getElementById('mood-img');
const moodBase = moodImg.dataset.base;
const moodDropdown = document.querySelector('[data-id="avatar-dropdown"]');
moodDropdown.onchange = function () {
	const moodImgElem = document.getElementById("mood-img");
	const val = moodDropdown.value;
	const replaced = moodBase.replace("$", val);
	moodImgElem.setAttribute("src", replaced);
};