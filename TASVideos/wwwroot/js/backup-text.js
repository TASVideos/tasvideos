// data-backup-content                  | textarea to save text from, needs a data-backup-key attribute
// id: "backup-time"                    | where the time will be put
// id: "backup-restore"                 | surrounding element hidden by default to display when backup exists
// id: "backup-restore-button"          | button to restore data
// id: "backup-submission-determinator" | span whose value only changes when a form submission went through (e.g. post count), it's used to determine whether a previous saved backup is deleted or not
{
	function convertSecondsToRelativeTime(seconds) {
		const minutes = Math.floor(seconds / 60);
		const hours = Math.floor(seconds / (60 * 60));
		const days = Math.floor(seconds / (60 * 60 * 24));
		if (seconds < 5) {
			return "a few seconds ago";
		}

		if (seconds < 60) {
			return `${seconds} seconds ago`;
		}

		if (minutes < 2) {
			return "1 minute ago";
		}

		if (minutes < 60) {
			return `${minutes} minutes ago`;
		}

		if (hours < 2) {
			return "1 hour ago";
		}

		if (hours < 24) {
			return `${hours} hours ago`;
		}

		if (days < 2) {
			return "1 day ago";
		}

		return `${days} days ago`;
	}

	const textarea = document.querySelector('textarea[data-backup-content="true"]');
	const submissionDeterminator = document.getElementById('backup-submission-determinator').innerHTML;
	const backupKey = textarea.dataset.backupKey;
	localStorage.removeItem(backupKey + '-restore');
	const restoreButton = document.getElementById('backup-restore-button');

	function updateRestoreButtonDisabledState() {
		restoreButton.disabled = !!textarea.value;
	}
	textarea.oninput = updateRestoreButtonDisabledState;

	let backupData = localStorage.getItem(backupKey);
	if (backupData) {
		let backupObject = JSON.parse(backupData);
		if (submissionDeterminator === backupObject.submissionDeterminator) {
			document.getElementById('backup-time').innerText = convertSecondsToRelativeTime(Math.floor(Date.now() / 1000) - backupObject.date);
			localStorage.setItem(backupKey + '-restore', backupData);
			updateRestoreButtonDisabledState();
			document.getElementById('backup-restore').classList.remove('d-none');
		} else {
			localStorage.removeItem(backupKey);
		}
	}

	function backupContent() {
		if (textarea.value) {
			const backupObject = JSON.stringify({
				content: textarea.value,
				date: Math.floor(Date.now() / 1000),
				submissionDeterminator: submissionDeterminator
			});
			localStorage.setItem(backupKey, backupObject);
		}
	}
	document.onvisibilitychange = () => {
		if (document.visibilityState === 'hidden') {
			backupContent();
		}
	};

	function restoreContent() {
		const backupObject = JSON.parse(localStorage.getItem(backupKey + '-restore'));
		if (backupObject && !textarea.value) {
			textarea.value = backupObject.content;
			updateRestoreButtonDisabledState();
		}
	}

	restoreButton.onclick = restoreContent;
}