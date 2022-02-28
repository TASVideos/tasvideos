function enableUserFile(gameId, systemId) {
	const systemModel = document.getElementById(systemId);
	const gameModel = document.getElementById(gameId);

	systemModel.onchange = function () {
		if (this.value) {
			window.fetch(`/Games/List/GameDropDownForSystem?includeEmpty=true&systemId=${systemModel.value}`)
				.then(r => r.text())
				.then(d => gameModel.innerHTML = d);
		} else {
			clearDropdown(gameId);
		}
	}
}