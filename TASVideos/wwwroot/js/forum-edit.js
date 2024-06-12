Array.from(document.querySelectorAll('[data-id="move-up"]')).forEach(moveUpBtn => {
	moveUpBtn.addEventListener('click', () => decrementOrdinal(moveUpBtn));
});

Array.from(document.querySelectorAll('[data-id="move-down"]')).forEach(moveDownBtn => {
	moveDownBtn.addEventListener('click', () => incrementOrdinal(moveDownBtn));
});

function incrementOrdinal(element) {
	const parent = element.closest('.forum-section');
	const index = parseInt(parent.dataset.index);

	const last = document.querySelectorAll('.forum-section').length;
	if (index >= last - 1) {
		return;
	}

	const thisO = document.getElementById(`Category_Forums_${index}__Ordinal`);
	const nextO = document.getElementById(`Category_Forums_${index + 1}__Ordinal`);

	const thisOVal = thisO.value;
	const nextOVal = nextO.value;

	thisO.value = nextOVal;
	nextO.value = thisOVal;

	const nextSection = document.querySelector(`.forum-section[data-index="${index + 1}"]`);

	parent.dataset.index = index + 1;
	nextSection.dataset.index = index;

	SortForums();
}

function decrementOrdinal(element) {
	const parent = element.closest('.forum-section');
	const index = parseInt(parent.dataset.index);
	if (index <= 0) {
		return;
	}

	const thisO = document.getElementById(`Category_Forums_${index}__Ordinal`);
	const prevO = document.getElementById(`Category_Forums_${index - 1}__Ordinal`);

	const thisOVal = thisO.value;
	const prevOVal = prevO.value;

	thisO.value = prevOVal;
	prevO.value = thisOVal;

	const prevSection = document.querySelector(`.forum-section[data-index="${index - 1}"]`);
	parent.dataset.index = index - 1;
	prevSection.dataset.index = index;

	SortForums();
}

function SortForums() {
	const sections = Array.from(document.querySelectorAll('.forum-section'))
		.sort((a, b) => a.dataset.index - b.dataset.index);

	const container = document.getElementById('forum-container');
	while (container.firstChild) {
		container.removeChild(container.firstChild);
	}

	for (const i in sections) {
		const ord = sections[i].querySelectorAll('[id$="Ordinal"]')[0];
		ord.setAttribute('id', `Category_Forums_${i}__Ordinal`);
		ord.setAttribute('name', `Category.Forums[${i}].Ordinal`);

		const id = sections[i].querySelectorAll('[id$="Id"]')[0];
		id.setAttribute('id', `Category_Forums_${i}__Id`);
		id.setAttribute('name', `Category.Forums[${i}].Id`);

		const name = sections[i].querySelectorAll('[id$="Name"]')[0];
		name.setAttribute('id', `Category_Forums_${i}__Name`);
		name.setAttribute('name', `Category.Forums[${i}].Name`);

		const description = sections[i].querySelectorAll('[id$="Description"]')[0];
		description.setAttribute('id', `Category_Forums_${i}__Description`);
		description.setAttribute('name', `Category.Forums[${i}].Description`);

		container.appendChild(sections[i]);
	}
}