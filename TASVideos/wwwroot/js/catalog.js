window.addEventListener("DOMContentLoaded", function () {
	enableCataloging();
});

function enableCataloging() {
	const systemModel = document.querySelector('[data-id="system"]');
	const frameRateModel = document.querySelector('[data-id="system-framerate"]');
	const gameModel = document.querySelector('[data-id="game"]');
	const versionModel = document.querySelector('[data-id="version"]');
	const createVersionBtn = document.getElementById('create-version');
	const gameGoalModel = document.querySelector('[data-id="goal"]');
	const gameGoalBtn = document.getElementById('create-goal');
	const returnUrlPathAndQuery = systemModel.dataset.returnUrl.split(/\?(.*)/s) // splits string at the first question mark (?)
	const returnUrlPath = returnUrlPathAndQuery[0];
	const returnUrlQuery = returnUrlPathAndQuery[1] ?? "";

	systemModel.onchange = function () {
		if (this.value) {
			if (gameModel) {
				fetch(`/Games/List/GameDropDownForSystem?includeEmpty=true&systemId=${systemModel.value}`)
					.then(handleFetchErrors)
					.then(r => r.text())
					.then(t => gameModel.innerHTML = t);
			}

			if (frameRateModel) {
				fetch(`/Games/List/FrameRateDropDownForSystem?includeEmpty=true&systemId=${systemModel.value}`)
					.then(handleFetchErrors)
					.then(r => r.text())
					.then(t => frameRateModel.innerHTML = t);
			}
		} else {
			clearDropdown(gameModel?.id);
			clearDropdown(frameRateModel?.id);
			clearDropdown(gameGoalModel?.id);
		}

		clearDropdown(versionModel?.id);
	}

	gameModel.onchange = function () {
		if (this.value) {
			createVersionBtn?.removeAttribute('disabled');
			createVersionBtn?.classList.remove('disabled');
			gameGoalBtn?.removeAttribute('disabled');
			gameGoalBtn?.classList.remove('disabled');
			if (versionModel) {
				fetch(`/Games/List/VersionDropDownForGame?includeEmpty=true&gameId=${gameModel.value}&systemId=${systemModel.value}`)
					.then(handleFetchErrors)
					.then(r => r.text())
					.then(t => versionModel.innerHTML = t);
			}
			
			if (gameGoalModel) {
				fetch(`/Games/List/GameGoalDropDownForGame?includeEmpty=false&gameId=${gameModel.value}`)
					.then(handleFetchErrors)
					.then(r => r.text())
					.then(t => gameGoalModel.innerHTML = t);
			}
		} else {
			createVersionBtn.classList.add('disabled');
			createVersionBtn.setAttribute('disabled', 'disabled');
			gameGoalBtn.classList.add('disabled');
			gameGoalBtn.setAttribute('disabled', 'disabled');
			clearDropdown(versionElemId);
			clearDropdown(gameGoalElemId);
		}
	}

	document.getElementById('create-version')?.addEventListener('click', function () {
		const returnUrlQueryModified = new URLSearchParams();
		returnUrlQueryModified.set('SystemId', systemModel.value);
		returnUrlQueryModified.set('GameId', gameModel.value);
		document.location = `/Games/${gameModel.value}/Versions/Edit?SystemId=${systemModel.value}&returnUrl=${encodeURIComponent(returnUrlPath + '?' + returnUrlQueryModified.toString())}`;
	});

	document.getElementById('create-game')?.addEventListener('click', function () {
		const returnUrlQueryModified = new URLSearchParams();
		returnUrlQueryModified.set('SystemId', systemModel.value);
		if (gameModel.value) {
			returnUrlQueryModified.set('GameId', gameModel.value);
		}
		document.location = `/Games/Edit?returnUrl=${encodeURIComponent(returnUrlPath + '?' + returnUrlQueryModified.toString())}`;
	});

	gameGoalBtn?.addEventListener('click', function () {
		const returnUrlQueryModified = new URLSearchParams();
		returnUrlQueryModified.set('SystemId', systemModel.value);
		returnUrlQueryModified.set('GameId', gameModel.value);
		if (versionModel.value) {
			returnUrlQueryModified.set('GameVersionId', versionModel.value);
		}
		if (gameGoalModel.value) {
			returnUrlQueryModified.set('GameGoalId', gameGoalModel.value);
		}
		document.location = `/Games/${gameModel.value}/Goals/List?returnUrl=${encodeURIComponent(returnUrlPath + '?' + returnUrlQueryModified.toString())}`;
	});
}