const systemModel = document.querySelector('[data-id="system"]');
const gameModel = document.querySelector('[data-id="game"]');

systemModel.onchange = function () {
	if (this.value) {
		fetch(`/Games/List/GameDropDownForSystem?includeEmpty=true&systemId=${systemModel.value}`)
			.then(handleFetchErrors)
			.then(r => r.text())
			.then(d => gameModel.innerHTML = d);
	} else {
		clearDropdown(gameModel.id);
	}
}
