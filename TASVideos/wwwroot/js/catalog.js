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
	const returnUrl = systemModel.dataset.returnUrl;

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
			clearDropdown(versionElemId);
			clearDropdown(gameGoalElemId);
		}
	}

	document.getElementById('create-version')?.addEventListener('click', function () {
		document.location = `/Games/${gameModel.value}/Versions/Edit?returnUrl=${encodeURIComponent(returnUrl)}&systemId=${systemModel.value}`;
	});

	document.getElementById('create-game')?.addEventListener('click', function () {
		if (returnUrl) {
			// we do not want to pass query params
			const split = returnUrl.split('?')[0];
			document.location = `/Games/Edit?returnUrl=${encodeURIComponent(split)}`;
		} else {
			document.location = '/Games/Edit';
		}

	});

	gameGoalBtn?.addEventListener('click', function () {
		document.location = `/Games/${gameModel.value}/Goals/List?returnUrl=${encodeURIComponent(returnUrl)}`;
	});
}