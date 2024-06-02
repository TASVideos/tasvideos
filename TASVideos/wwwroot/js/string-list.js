document.addEventListener("DOMContentLoaded", findAndEngageStringLists);

function findAndEngageStringLists() {
	const containers = Array.from(document.querySelectorAll('.string-list-container'));
	containers.forEach(container => {
		engageStringList(container);
	});
}

function engageStringList(container) {
	const addBtn = container.querySelector('.string-list-add-btn');
	addBtn.onclick = addBtnClicked.bind(addBtn, container);

	Array.from(container.querySelectorAll('.string-list-row')).forEach(row => {
		const moveUpBtn = row.querySelector('.move-up-btn');
		moveUpBtn.onclick = upArrowClicked.bind(moveUpBtn);

		const moveDownBtn = row.querySelector('.move-down-btn');
		moveDownBtn.onclick = downArrowClicked.bind(moveDownBtn);

		const deleteBtn = row.querySelector('.delete-entry-btn');
		deleteBtn.onclick = deleteBtnClicked.bind(deleteBtn);
	});
}

function addBtnClicked(container) {
	const lastIndex = Math.max.apply(null, Array.from(container.querySelectorAll('.string-list-row'))
		.map(element => parseInt(element.getAttribute('data-index'))));

	const lastElem = container.querySelector(`[data-index="${lastIndex }"]`);

	const newIndex = lastIndex + 1;
	let newElem = lastElem.cloneNode(true);
	newElem.setAttribute('data-index', newIndex);
	let input = newElem.querySelector('input');
	input.value = '';
	input.id = 'Authors_' + newIndex + '_';

	const moveUpBtn = newElem.querySelector('.move-up-btn');
	moveUpBtn.onclick = upArrowClicked.bind(moveUpBtn);

	const moveDownBtn = newElem.querySelector('.move-down-btn');
	moveDownBtn.onclick = downArrowClicked.bind(moveDownBtn);

	const deleteBtn = newElem.querySelector('.delete-entry-btn');
	deleteBtn.onclick = deleteBtnClicked.bind(deleteBtn);

	container.insertBefore(newElem, this);
}

function upArrowClicked() {
	const fec = 'firstElementChild';
	const cur = this.parentElement.parentElement;
	const prv = cur.previousElementSibling;
	if (prv && prv.classList.contains('string-list-row')) {
		const tmp = cur[fec][fec].value;
		cur[fec][fec].value=prv[fec][fec].value;
		prv[fec][fec].value=tmp;
	}
}

function downArrowClicked() {
	const fec= 'firstElementChild';
	const cur= this.parentElement.parentElement;
	const nxt= cur.nextElementSibling;
	if (nxt && nxt.classList.contains('string-list-row')) {
		const tmp=cur[fec][fec].value;
		cur[fec][fec].value = nxt[fec][fec].value;
		nxt[fec][fec].value=tmp;
	}
}

function deleteBtnClicked() {
	const container = this.closest('.string-list-container');
	if (container.querySelectorAll('.string-list-row').length > 1 ) {
		this.parentElement.parentElement.remove();
	}
}
