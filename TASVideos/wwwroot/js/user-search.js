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
	const getCurrentValue = () => searchBoxElem.value.trim();

	function maybeEnableSubmit() {
		if (submitBtnElem) {
			submitBtnElem.disabled = !validNames.has(getCurrentValue());
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
			validNames.add(userName);
		}
		updateDataList(data);
		maybeEnableSubmit();
	});
}
