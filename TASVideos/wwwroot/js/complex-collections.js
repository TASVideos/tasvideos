function replaceAttributes(element, search, replace) {
	for (let i = 0; i < element.attributes.length; i++) {
		let attr = element.attributes[i];
		if (attr.nodeValue.startsWith(search)) {
			element.setAttribute(attr.nodeName, attr.nodeValue.replace(search, replace));
		}
	}

	for (let i = 0; i < element.children.length; i++) {
		replaceAttributes(element.children[i], search, replace);
	}
}

function decreaseAttributes(element, prefix) {
	var regex = new RegExp(`\\d+`);
	for (let i = 0; i < element.attributes.length; i++) {
		let attr = element.attributes[i];
		if (attr.nodeValue.startsWith(prefix)) {
			element.setAttribute(attr.nodeName, attr.nodeValue.replace(prefix, '').replace(regex, (number) => `${prefix}${parseInt(number) - 1}`));
		}
	}

	for (let i = 0; i < element.children.length; i++) {
		decreaseAttributes(element.children[i], prefix);
	}
}

function addCollectionEntry(collectionId) {
	var collectionTemplate = document.querySelector(`.template-collection[data-for='${collectionId}']`);
	var cloned = collectionTemplate.content.cloneNode(true);
	var newEntry = document.createElement('div')
	newEntry.appendChild(cloned);

	var collection = document.getElementById('collection-' + collectionId)
	var entryCount = collection.children.length;

	var regexDefault = new RegExp('Default$');
	replaceAttributes(newEntry, collectionId, collectionId.replace(regexDefault, `[${entryCount}]`));
	var collectionIdUnderscore = collectionId.replaceAll('.', '_');
	replaceAttributes(newEntry, collectionIdUnderscore, collectionIdUnderscore.replace(regexDefault, `_${entryCount}_`));

	collection.appendChild(newEntry);
}

function resetFormValidator() {
	$('form').removeData('validator');
	$('form').removeData('unobtrusiveValidation');
	$.validator.unobtrusive.parse('form');
}

function getAllNextElementSiblings(element) {
	var nextElements = [];
	var runningElement = element;
	while (runningElement.nextElementSibling) {
		nextElements.push(runningElement.nextElementSibling);
		runningElement = runningElement.nextElementSibling;
	}
	return nextElements;
}

function addRemoveButtonEventListeners() {
	Array.from(document.getElementsByClassName('remove-collection-entry')).forEach((btn) => {
		btn.classList.remove('remove-collection-entry');
		btn.addEventListener('click', () => {
			var collectionId = btn.closest("[id^='collection-']").id.replace('collection-', '');
			var currentEntry = btn.closest("[id^='collection-'] > div");
			var nextEntries = getAllNextElementSiblings(currentEntry);
			currentEntry.remove();
			var regexDefault = new RegExp('Default$');
			for (var nextEntry of nextEntries) {
				decreaseAttributes(nextEntry, collectionId.replace(regexDefault, '') + '[');
				decreaseAttributes(nextEntry, collectionId.replace(regexDefault, '').replace('.', '_') + '_');
			}
		});
	});
}

addRemoveButtonEventListeners();
Array.from(document.getElementsByClassName('add-collection-entry')).forEach((btn) => {
	btn.addEventListener('click', () => {
		addCollectionEntry(btn.dataset.for);
		addRemoveButtonEventListeners();
		resetFormValidator();
	});
});