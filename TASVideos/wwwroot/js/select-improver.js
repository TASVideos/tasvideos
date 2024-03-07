function createButtonElement(text, value) {
	const button = document.createElement('button');
	button.type = 'button';
	button.classList.add('btn', 'btn-primary', 'btn-sm', 'mb-1', 'me-1');
	button.dataset.value = value;

	const buttonSpanText = document.createElement('span');
	buttonSpanText.innerText = text;
	button.appendChild(buttonSpanText);
	const buttonSpanX = document.createElement('span');
	buttonSpanX.innerText = '✕';
	buttonSpanX.classList.add('ps-1');
	button.appendChild(buttonSpanX);

	return button;
}
function toggleSelectOption(multiSelect, buttons, optionsList, value, dispatchEvent = true, option = null) {
	let element;
	let buttonsBefore = 0;
	for (let o of optionsList) {
		const input = o.querySelector('input');
		if (input.dataset.value === value) {
			element = input;
			break;
		}
		if (input.checked) {
			buttonsBefore++;
		}
	}

	option = option || [...multiSelect.options].find(o => o.value === value);
	if (option.selected) {
		option.selected = false;
		element.checked = false;
		buttons.querySelector(`button[data-value='${value}']`).remove();
		if (![...multiSelect.options].some(o => o.selected)) {
			buttons.querySelector('span').classList.remove('d-none');
		}
	} else {
		option.selected = true;
		element.checked = true;
		buttons.insertBefore(createButtonElement(option.text, option.value), buttons.querySelectorAll('button')[buttonsBefore]);
		buttons.querySelector('span').classList.add('d-none');
	}

	if ([...multiSelect.options].some(o => !o.selected)) {
		buttons.querySelector('a').querySelector('i').classList.replace('fa-minus', 'fa-plus');
		buttons.querySelector('a').title = 'Select All';
	} else {
		buttons.querySelector('a').querySelector('i').classList.replace('fa-plus', 'fa-minus');
		buttons.querySelector('a').title = 'Deselect All';
	}

	if (dispatchEvent) {
		multiSelect.dispatchEvent(new Event('change')); // somewhat hacky way to support external event listeners
	}
}
function renderVirtualScroll(list, optionsList, visibleHeight) {
	const firstElementHeight = 39;
	const otherElementsHeight = 38;
	
	const scrollPosition = list.scrollTop;

	const optionsListVisible = optionsList.filter(option => option.dataset.visible === String(true));
	list.innerHTML = '';

	const topIndex = scrollPosition < firstElementHeight ? 0 : Math.floor((scrollPosition - firstElementHeight) / otherElementsHeight) + 1;
	const topSpaceHeight = topIndex == 0 ? 0 : firstElementHeight + (topIndex - 1) * otherElementsHeight;
	let bottomIndex = (scrollPosition + visibleHeight) < firstElementHeight ? 0 : Math.floor(((scrollPosition + visibleHeight) - firstElementHeight) / otherElementsHeight) + 1;
	if (bottomIndex > optionsListVisible.length - 1) { bottomIndex = optionsListVisible.length - 1; }
	const bottomSpaceHeight = ((optionsListVisible.length - 1) - bottomIndex) * otherElementsHeight;

	const topSpace = document.createElement('div');
	topSpace.style.height = topSpaceHeight + 'px';
	const bottomSpace = document.createElement('div');
	bottomSpace.style.height = bottomSpaceHeight + 'px';
	list.appendChild(topSpace);
	for (let i = topIndex; i <= bottomIndex; i++) {
		list.appendChild(optionsListVisible[i]);
	}
	list.appendChild(bottomSpace);
}
function engageSelectImprover(multiSelectId, maxHeight = 250) {
	let initialHtmlToAdd = `
<div id='${multiSelectId}_div' class='d-none border bg-body rounded-2 onclick-focusinput' style='cursor: text;'>
	<div id='${multiSelectId}_buttons' class='onclick-focusinput px-2 pt-2 pb-1'>
		<span class='text-body-tertiary onclick-focusinput px-1'>No selection</span>
		<a class='btn btn-sm btn-outline-silver float-end py-0 px-1'></a>
	</div>
	<input id='${multiSelectId}_input' class='d-none form-control' placeholder='Search' autocomplete='off' />
	<div id='${multiSelectId}_list' class='list-group mt-1 overflow-auto border d-block' style='max-height: ${maxHeight}px;'></div>
</div>
`;
	let multiSelect = document.getElementById(multiSelectId);
	multiSelect.classList.add('d-none');
	let div = document.getElementById(multiSelectId + '_div');
	div?.remove();
	multiSelect.insertAdjacentHTML('afterend', initialHtmlToAdd);
	div = document.getElementById(multiSelectId + '_div');
	let list = document.getElementById(multiSelectId + '_list');
	let buttons = document.getElementById(multiSelectId + '_buttons');
	div.classList.remove('d-none');
	let input = document.getElementById(multiSelectId + '_input');
	let optionsList = [];
	let anyNotSelected = false;

	let entry = document.createElement('div');
	entry.classList.add('list-group-item', 'list-group-item-action', 'px-1', 'text-nowrap');
	entry.dataset.visible = true;
	let label = document.createElement('label');
	label.classList.add('form-check-label', 'stretched-link');
	let checkbox = document.createElement('input');
	checkbox.type = 'checkbox';
	checkbox.classList.add('form-check-input', 'ms-1', 'me-2');
	label.appendChild(checkbox);
	entry.appendChild(label);
	for (var option of multiSelect.options) {
		let newEntry = entry.cloneNode(true);
		let label = newEntry.childNodes[0];
		let checkbox = label.childNodes[0];
		if (option.selected) {
			checkbox.checked = true;
		}
		if (option.disabled) {
			checkbox.disabled = true;
			newEntry.classList.add('disabled');
		}
		checkbox.dataset.value = option.value;
		label.append(option.text);
		optionsList.push(newEntry);

		if (option.selected) {
			let button = createButtonElement(option.text, option.value);
			button.disabled = option.disabled;
			buttons.appendChild(button);
		}

		if (option.selected) {
			buttons.querySelector('span').classList.add('d-none');
		} else {
			anyNotSelected = true;
		}
	}

	let toggleAllIcon = document.createElement('i');
	toggleAllIcon.classList.add('fa', 'fa-sm');
	toggleAllIcon.classList.add(anyNotSelected ? 'fa-plus' : 'fa-minus');
	buttons.querySelector('a').appendChild(toggleAllIcon);
	buttons.querySelector('a').title = anyNotSelected ? 'Select All' : 'Deselect All';
	buttons.querySelector('a').addEventListener('click', () => {
		const a = getTicks();
		const notSelected = [...multiSelect.options].filter(o => !o.selected);
		if (notSelected.length) {
			for (let o of notSelected) {
				toggleSelectOption(multiSelect, buttons, optionsList, o.value, true, o);
			}
		} else {
			for (let o of multiSelect.options) {
				toggleSelectOption(multiSelect, buttons, optionsList, o.value, true, o);
			}
		}
		multiSelect.dispatchEvent(new Event('change')); // somewhat hacky way to support external event listeners
		const b = getTicks();
		console.log('ms', b - a);
	});
	div.addEventListener('click', (e) => {
		if (e.target.classList.contains('onclick-focusinput')) {
			input.classList.remove('d-none');
			input.focus();
		}
	});
	input.addEventListener('input', () => {
		const searchValue = input.value.toLowerCase();
		for (let option of optionsList) {
			option.dataset.visible = option.querySelector('label').innerText.toLowerCase().includes(searchValue);
		}
		list.dispatchEvent(new Event('scroll'));
	});
	list.addEventListener('change', (e) => {
		toggleSelectOption(multiSelect, buttons, optionsList, e.target.dataset.value);
	});
	buttons.addEventListener('click', (e) => {
		const button = e.target.closest('button');
		if (button) {
			toggleSelectOption(multiSelect, buttons, optionsList, button.dataset.value);
		}
	});
	input.addEventListener('focusout', () => {
		if (!input.value) {
			input.classList.add('d-none');
		}
	});
	list.addEventListener('scroll', (e) => renderVirtualScroll(e.target, optionsList, maxHeight - 2));
	list.dispatchEvent(new Event('scroll'));
}


function getTicks() {
	var d = new Date();
	var dStart = new Date(1970, 1, 1);
	var dateDifference = ((d.getTime() - dStart.getTime()));
	console.log(dateDifference); // 15198588000000000
	return dateDifference;
}