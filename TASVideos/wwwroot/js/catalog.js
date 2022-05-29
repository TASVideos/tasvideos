function enableCataloging(
	systemElemId,
	frameRateElemId,
	gameElemId,
	versionElemId,
	returnUrl) {
	const systemModel = document.getElementById(systemElemId);
	const frameRateModel = document.getElementById(frameRateElemId);
	const gameModel = document.getElementById(gameElemId);
	const versionModel = document.getElementById(versionElemId);
	const createVersionBtn = document.getElementById("create-version");

	systemModel.onchange = function () {
		if (this.value) {
			fetch(`/Games/List/GameDropDownForSystem?includeEmpty=true&systemId=${systemModel.value}`)
				.then(handleFetchErrors)
				.then(r => r.text())
				.then(t => gameModel.innerHTML = t);

			fetch(`/Games/List/FrameRateDropDownForSystem?includeEmpty=true&systemId=${systemModel.value}`)
				.then(handleFetchErrors)
				.then(r => r.text())
				.then(t => frameRateModel.innerHTML = t);
		} else {
			clearDropdown(gameElemId);
			clearDropdown(frameRateElemId);
		}

		clearDropdown(versionElemId);
	}

	gameModel.onchange = function () {
		if (this.value) {
			createVersionBtn.removeAttribute('disabled');
			createVersionBtn.classList.remove('disabled');
			fetch(`/Games/List/VersionDropDownForGame?includeEmpty=true&gameId=${gameModel.value}&systemId=${systemModel.value}`)
				.then(handleFetchErrors)
				.then(r => r.text())
				.then(t => versionModel.innerHTML = t);
		} else {
			createVersionBtn.classList.add('disabled');
			createVersionBtn.setAttribute('disabled', 'disabled');
			clearDropdown(versionElemId);
		}
	}

	document.getElementById('create-version').onclick = function () {
		document.location = `/Games/${gameModel.value}/Versions/Edit?returnUrl=${returnUrl}&systemId=${systemModel.value}`;
	}

	document.getElementById('create-game').onclick = function () {
		document.location = `/Games/Edit?returnUrl=${returnUrl}`;
	}
}