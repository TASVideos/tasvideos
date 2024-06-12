const changeColumns = document.querySelectorAll('td.change');
changeColumns.forEach(function (elem) {
	const before = elem.querySelector('.before');
	const after = elem.querySelector('.after');

	renderDiff(
		{ text: before.value, name: "Before" },
		{ text: after.value, name: "After" },
		elem,
		true,
		1);
});