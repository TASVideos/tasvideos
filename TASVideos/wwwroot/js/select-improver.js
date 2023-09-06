function toggleSelectOption(multiSelect, buttons, list, value, dispatchEvent = true) {
	const element = [...list.querySelectorAll('input')].find(el => el.dataset.value === value);
	const option = [...multiSelect.options].find(o => o.value === value);
	const button = [...buttons.querySelectorAll('button')].find(b => b.dataset.value === value);
	const isSelected = option.selected;
	if (isSelected) {
		option.selected = false;
		element.checked = false;
		button.classList.add('d-none');
		if (![...multiSelect.options].some(o => o.selected)) {
			buttons.querySelector('span').classList.remove('d-none');
		}
	} else {
		option.selected = true;
		element.checked = true;
		button.classList.remove('d-none');
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

function engageSelectImprover(multiSelectId, maxHeight = '250px') {
	let initialHtmlToAdd = `
<div id='${multiSelectId}_div' class='d-none border bg-body rounded-2 onclick-focusinput' style='cursor: text;'>
	<div id='${multiSelectId}_buttons' class='onclick-focusinput px-2 pt-2 pb-1'>
		<span class='text-body-tertiary onclick-focusinput px-1'>No selection</span>
		<a class='btn btn-sm btn-outline-silver float-end py-0 px-1'></a>
	</div>
	<input id='${multiSelectId}_input' class='d-none form-control' placeholder='Search' autocomplete='off' />
	<div id='${multiSelectId}_list' class='list-group mt-1 overflow-auto border' style='max-height: ${maxHeight};'></div>
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
	let anyNotSelected = false;
	for (var option of multiSelect.options) {
		let entry = document.createElement('div');
		entry.classList.add('list-group-item', 'list-group-item-action', 'px-1');
		let label = document.createElement('label');
		label.classList.add('form-check-label', 'stretched-link');
		let checkbox = document.createElement('input');
		checkbox.type = 'checkbox';
		checkbox.classList.add('form-check-input', 'ms-1', 'me-2');
		if (option.selected) {
			checkbox.checked = true;
		}
		if (option.disabled) {
			checkbox.disabled = true;
			entry.classList.add('disabled');
		}

		checkbox.dataset.value = option.value;
		checkbox.addEventListener('change', (e) => {
			toggleSelectOption(multiSelect, buttons, list, e.currentTarget.dataset.value);
		});

		label.appendChild(checkbox);
		label.append(option.text);
		entry.appendChild(label);
		list.appendChild(entry);

		let button = document.createElement('button');
		button.type = 'button';
		button.classList.add('btn', 'btn-primary', 'btn-sm', 'mb-1', 'me-1');
		button.dataset.value = option.value;
		if (option.selected) {
			buttons.querySelector('span').classList.add('d-none');
		} else {
			button.classList.add('d-none');
		}
		if (option.disabled) {
			button.disabled = true;
		}

		button.addEventListener('click', (e) => {
			toggleSelectOption(multiSelect, buttons, list, e.currentTarget.dataset.value);
		});

		let buttonSpanText = document.createElement('span');
		buttonSpanText.innerText = option.text;
		button.appendChild(buttonSpanText);
		let buttonSpanX = document.createElement('span');
		buttonSpanX.innerText = '✕';
		buttonSpanX.classList.add('ps-1');
		button.appendChild(buttonSpanX);

		buttons.appendChild(button);

		if (!option.selected) {
			anyNotSelected = true;
		}
	}

	let toggleAllIcon = document.createElement('i');
	toggleAllIcon.classList.add('fa', 'fa-sm');
	toggleAllIcon.classList.add(anyNotSelected ? 'fa-plus' : 'fa-minus');
	buttons.querySelector('a').appendChild(toggleAllIcon);
	buttons.querySelector('a').title = anyNotSelected ? 'Select All' : 'Deselect All';
	buttons.querySelector('a').addEventListener('click', () => {
		const notSelected = [...multiSelect.options].filter(o => !o.selected);
		if (notSelected.length) {
			for (let o of notSelected) {
				toggleSelectOption(multiSelect, buttons, list, o.value, false);
			}
		} else {
			for (let o of multiSelect.options) {
				toggleSelectOption(multiSelect, buttons, list, o.value, false);
			}
		}
		multiSelect.dispatchEvent(new Event('change')); // somewhat hacky way to support external event listeners
	});
	div.addEventListener('click', (e) => {
		if (e.target.classList.contains('onclick-focusinput')) {
			input.classList.remove('d-none');
			input.focus();
		}
	});
	input.addEventListener('input', () => {
		const searchValue = input.value.toLowerCase();
		for (let entry of list.querySelectorAll('label')) {
			if (entry.innerText.toLowerCase().includes(searchValue)) {
				entry.parentNode.classList.remove('d-none');
			} else {
				entry.parentNode.classList.add('d-none');
			}
		}
	});
	input.addEventListener('focusout', () => {
		if (!input.value) {
			input.classList.add('d-none');
		}
	});
}