// class: "backup-form"        | surrounding form
// class: "backup-content"     | textarea to save text from, needs a data-backup-key attribute
// id: "backup-time"           | where the time will be put
// id: "backup-restore"        | surrounding element hidden by default to display when backup exists
// id: "backup-restore-button" | button to restore data

function convertSecondsToRelativeTime(seconds) {
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(seconds / (60 * 60));
    const days = Math.floor(seconds / (60 * 60 * 24));
    if (seconds < 5) {
        return "a few seconds ago";
    }
    else if (seconds < 60) {
        return `${seconds} seconds ago`;
    }
    else if (minutes < 2) {
        return "1 minute ago";
    }
    else if (minutes < 60) {
        return `${minutes} minutes ago`;
    }
    else if (hours < 2) {
        return "1 hour ago";
    }
    else if (hours < 24) {
        return `${hours} hours ago`;
    }
    else if (days < 2) {
        return "1 day ago";
    }
    else {
        return `${days} days ago`;
    }
}

const textarea = document.querySelector('textarea.backup-content');
const backupKey = textarea.dataset.backupKey;
localStorage.removeItem(backupKey + '-restore');

let backupData = localStorage.getItem(backupKey);
if (backupData) {
    let backupObject = JSON.parse(backupData);
    document.getElementById('backup-time').innerText = convertSecondsToRelativeTime(Math.floor(Date.now() / 1000) - backupObject.date);
    localStorage.setItem(backupKey + '-restore', backupData);
    document.getElementById('backup-restore').classList.remove('d-none');
}

function backupContent() {
    if (textarea.value) {
        let backupObject = JSON.stringify({ content: textarea.value, date: Math.floor(Date.now() / 1000) });
        localStorage.setItem(backupKey, backupObject)
    }
}
setInterval(backupContent, 5000);

const restoreButton = document.getElementById('backup-restore-button');
function restoreContent() {
    let backupObject = JSON.parse(localStorage.getItem(backupKey + '-restore'));
    if (backupObject && !textarea.value) {
        textarea.value = backupObject.content;
        backupContent();
    }
}
restoreButton.onclick = restoreContent;

const form = document.querySelector('form.backup-form');
function deleteBackup() {
    localStorage.removeItem(backupKey);
    localStorage.removeItem(backupKey + '-restore');
}
form.onsubmit = deleteBackup;