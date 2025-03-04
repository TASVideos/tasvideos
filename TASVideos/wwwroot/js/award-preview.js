window.addEventListener("DOMContentLoaded", function () {
	const awardDropdown = document.querySelector('[data-id="award-dropdown"]');
	awardDropdown.addEventListener('change', awardDropdownChanged);

	const awardImg = document.getElementById('award-image');
	awardImg.onerror = () => {
		awardImg.src = '/images/empty.png';
	};

	awardDropdownChanged();
});

function awardDropdownChanged() {
	const awardDropdown = document.querySelector('[data-id="award-dropdown"]');
	const year = awardDropdown.dataset.year;
	const path = `/awards/${year}/${awardDropdown.value}_${year}.png`;
	const imgElem = document.getElementById('award-image');
	imgElem.src = path;
}