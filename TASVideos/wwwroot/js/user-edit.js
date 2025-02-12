const checkUserBtn = document.getElementById('check-username');
const userNameBox = document.querySelector('[data-id="username"]');
const userNameStatus = document.getElementById('user-name-status');
const userNameExistsMessage = document.getElementById('username-exists-message');
const originalUserNameBox = document.querySelector('[data-id="original-username"]');
const userNameDiv = document.getElementById('user-name-div');

userNameBox.onkeyup = onUserNameBoxChange;
userNameBox.onchange = onUserNameBoxChange;

function onUserNameBoxChange() {
	if (userNameBox.value === originalUserNameBox.value) {
		hideCheckNameBtn();
		markUserNameGood();
	} else {
		showCheckNameBtn();
		markUserNameUnknown();
	}
}

function showCheckNameBtn() {
	checkUserBtn.classList.remove('d-none');
	checkUserBtn.parentNode.classList.add('col-sm-2');
	userNameDiv.classList.remove('col-sm-12');
	userNameDiv.classList.add('col-sm-10');
}

function hideCheckNameBtn() {
	checkUserBtn.classList.add("d-none");
	checkUserBtn.parentNode.classList.remove('col-sm-2');
	userNameDiv.classList.remove('col-sm-10');
	userNameDiv.classList.add('col-sm-12');
}

function markUserNameUnknown() {
	userNameStatus.classList.remove('fa-check-square', 'text-success');
	userNameStatus.classList.remove('fa-exclamation-triangle', 'text-danger');
	userNameStatus.classList.add('fa-question-circle', 'text-primary');
	userNameExistsMessage.classList.add('d-none');
	document.getElementById('submit-btn').disabled = true;
}

function markUserNameGood() {
	userNameStatus.classList.add('fa-check-square', 'text-success');
	userNameStatus.classList.remove('fa-exclamation-triangle', 'text-danger');
	userNameStatus.classList.remove('fa-question-circle', 'text-primary');
	userNameExistsMessage.classList.add('d-none');
	document.getElementById('submit-btn').disabled = false;
}

function markUserNameBad() {
	userNameStatus.classList.remove('fa-check-square', 'text-success');
	userNameStatus.classList.add('fa-exclamation-triangle', 'text-danger');
	userNameStatus.classList.remove('fa-question-circle', 'text-primary');
	userNameExistsMessage.classList.remove('d-none');
	document.getElementById('submit-btn').disabled = true;
}

checkUserBtn.onclick = function () {
	if (originalUserNameBox.value === userNameBox.value) {
		markUserNameGood();
		return;
	}

	fetch(`/Users/List?handler=CanRenameUser&oldUserName=${originalUserNameBox.value}&newUserName=${userNameBox.value}`)
		.then(handleFetchErrors)
		.then(r => r.text())
		.then(d => {
			if (d === "true") {
				markUserNameGood();
			} else {
				markUserNameBad();
			}
		});
};