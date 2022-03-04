function searchUsers(searchBoxElemId) {
	const searchBoxElem = document.getElementById(searchBoxElemId);
	const formElem = searchBoxElem.closest("form");
	const submitBtnElem = formElem.querySelector("button[type='Submit']");
	searchBoxElem.onkeyup = function () {
		const searchVal = searchBoxElem.value.trim();
		const dataListId = `search-username-list-${searchVal}`;
		if (searchVal.length > 2) {
			const existingList = document.getElementById(dataListId);
			if (existingList) {
				searchBoxElem.setAttribute("list", dataListId);
				return;
			}

			fetch(`/Users/List/Search/?partial=${searchVal}`)
				.then(handleFetchErrors)
				.then(r => r.json())
				.then(data => {
					const newSearchList = document.createElement("datalist");
					newSearchList.id = dataListId;
					for (const i in data) {
						if (data.hasOwnProperty(i)) {
							const option = document.createElement("option");
							option.innerHTML = data[i];
							newSearchList.appendChild(option);
						}
					}

					formElem.appendChild(newSearchList);
					searchBoxElem.setAttribute("list", dataListId);

					if (submitBtnElem) {
						submitBtnElem.removeAttribute("disabled");
					}
				});
		} else if (submitBtnElem) {
			submitBtnElem.setAttribute("disabled", "disabled");
		}
	}
}