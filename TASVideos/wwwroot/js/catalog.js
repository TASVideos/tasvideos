﻿function enableCataloging(
	systemElemId,
	frameRateElemId,
	gameElemId,
	romElemId,
	returnUrl) {
	const systemModel = document.getElementById(systemElemId);
	const frameRateModel = document.getElementById(frameRateElemId);
	const gameModel = document.getElementById(gameElemId);
	const romModel = document.getElementById(romElemId);
	const createRomBtn = document.getElementById("create-rom");

	systemModel.onchange = function () {
		if (this.value) {
			window.fetch(`/Games/List/GameDropDownForSystem?includeEmpty=true&systemId=${systemModel.value}`)
				.then(r => r.text())
				.then(t => gameModel.innerHTML = t);

			window.fetch(`/Games/List/FrameRateDropDownForSystem?includeEmpty=true&systemId=${systemModel.value}`)
				.then(r => r.text())
				.then(t => frameRateModel.innerHTML = t);
		} else {
			clearDropdown(gameElemId);
			clearDropdown(frameRateElemId);
		}

		clearDropdown(romElemId);
	}

	gameModel.onchange = function () {
		if (this.value) {
			createRomBtn.removeAttribute('disabled');
			window.fetch(`/Games/List/RomDropDownForGame?includeEmpty=true&gameId=${gameModel.value}`)
				.then(r => r.text())
				.then(t => romModel.innerHTML = t);
		} else {
			createRomBtn.setAttribute('disabled', 'disabled');
			clearDropdown(romElemId);
		}
	}

	document.getElementById('create-rom').onclick = function () {
		document.location = `/Games/${gameModel.value}/Roms/Edit?returnUrl=${returnUrl}`;
	}

	document.getElementById('create-game').onclick = function () {
		document.location = `/Games/Edit?returnUrl=${returnUrl}&systemId=${systemElemId.value}`;
	}

	function clearDropdown(elemId) {
		Array.from(document.querySelectorAll(`#${elemId} option`))
			.forEach(element => {
				if (element.value) {
					element.remove();
				}
			});
	}
}