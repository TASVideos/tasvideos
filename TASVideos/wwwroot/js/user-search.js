document.addEventListener("DOMContentLoaded", wireUpUserSearchBoxes);
function wireUpUserSearchBoxes() {
	const userSearches = Array.from(document.querySelectorAll('[data-user-search="true"]'));
	userSearches.forEach(search => {
		searchUsers(search.id);
	})
}

function searchUsers(searchBoxElemId) {
	const searchBoxElem = document.getElementById(searchBoxElemId);
	const formElem = searchBoxElem.closest("form");
	const submitBtnElem = formElem.querySelector("button[type='Submit']");
	const dataList = document.createElement("datalist");
	dataList.id = "search-username-list";
	formElem.appendChild(dataList);
	searchBoxElem.setAttribute("list", "search-username-list");

	const validNames = new Set;
	const nameLists = new Map;
	const getCurrentValue = () => searchBoxElem.value;

	function maybeEnableSubmit() {
		if (submitBtnElem) {
			const valid = validNames.has(getCurrentValue().toUpperCase()) && getCurrentValue().length > 0;
			if (valid) {
				submitBtnElem.removeAttribute('disabled');
				submitBtnElem.removeAttribute('tabIndex');
				submitBtnElem.removeAttribute('aria-disabled');
				submitBtnElem.classList.remove('disabled');
			} else {
				submitBtnElem.setAttribute('disabled', 'disabled');
				submitBtnElem.setAttribute('tabIndex', '-1');
				submitBtnElem.setAttribute('aria-disabled', 'disabled');
				submitBtnElem.classList.add('disabled');
			}
		}
	}
	function updateDataList(names) {
		dataList.innerHTML = "";
		for (const userName of names) {
			const option = document.createElement("option");
			option.textContent = userName;
			dataList.appendChild(option);
		}
	}

	searchBoxElem.addEventListener("input", async () => {
		maybeEnableSubmit();
		const value = getCurrentValue();
		if (value.length <= 2) {
			updateDataList([]);
			return;
		}

		const existingNames = nameLists.get(value);
		if (existingNames) {
			updateDataList(existingNames);
			return;
		}

		const res = handleFetchErrors(await fetch(`/Users/List/Search/?partial=${value}`));
		const data = await res.json();

		nameLists.set(value, data);
		for (const userName of data) {
			validNames.add(userName.toUpperCase());
		}
		updateDataList(data);
		maybeEnableSubmit();
	});
}
