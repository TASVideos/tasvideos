const selectedPermissionsModelName = 'Role.SelectedPermissions';
const selectedAssignableModelName = 'Role.SelectedAssignablePermissions';
const assignRolesValue = '504';
const id = document.getElementById('Id').value
const selectedPermElem = document.querySelector(`[name="${selectedPermissionsModelName}"]`);
const selectedAssignablePermElem = document.querySelector(`[name="${selectedAssignableModelName}"]`);

function shouldShowAssignRoles() {
	const hasAssign = Boolean(document.querySelector(`[name="${selectedPermissionsModelName}"] option:checked[value="${assignRolesValue}"]`));
	const hasAtLeastOneOtherPerm = document.querySelectorAll(`[name="${selectedPermissionsModelName}"] option:checked`).length > 1;

	return hasAssign && hasAtLeastOneOtherPerm;
}

function syncAssignablePerms() {
	// remove all non-selected permissions from assignable permissions
	[...selectedAssignablePermElem.options]
		.filter(option => ![...selectedPermElem.options]
			.filter(o => o.selected)
			.find(o => o.value == option.value))
		.forEach(option => option.remove());

	// add all missing selected permissions to assignable permissions
	[...selectedPermElem.options]
		.filter(option => option.selected && ![...selectedAssignablePermElem.options]
			.find(o => o.value == option.value))
		.forEach(option => {
			const newOption = document.createElement('option');
			newOption.value = option.value;
			newOption.text = option.text;
			selectedAssignablePermElem.add(newOption);
		});

	engageSelectImprover('Role_SelectedAssignablePermissions');

	showRolesCurrentlyAssignable();
}

function showRolesCurrentlyAssignable() {
	const permissions = Array
		.from(document.querySelectorAll(`[name="${selectedAssignableModelName}"] option:checked`))
		.map(element => element.value);

	if (!permissions.length) {
		document.getElementById("assignable-roles").textContent = 'None';
		return;
	}
	const selectedOptions = [...selectedAssignablePermElem].filter(option => option.selected).map(option => option.value);
	const url = `/Roles/AddEdit/${id}?handler=RolesThatCanBeAssignedBy&ids=${selectedOptions.join('&ids=')}`;
	fetch(url)
		.then(handleFetchErrors)
		.then(r => r.json())
		.then(json => {
			if (json.length > 0) {
				document.getElementById("assignable-roles").textContent = json.join(', ');
			} else {
				document.getElementById("assignable-roles").textContent = 'None';
			}
		});
}

function onSelectedPermissionsChange() {
	if (shouldShowAssignRoles()) {
		document.getElementById('assignable-permissions-section').classList.remove('d-none');
		syncAssignablePerms();
	} else {
		document.getElementById('assignable-permissions-section').classList.add('d-none');
	}
}

selectedPermElem.addEventListener('change', onSelectedPermissionsChange);
document.addEventListener("DOMContentLoaded", onSelectedPermissionsChange);
selectedAssignablePermElem.addEventListener('change', showRolesCurrentlyAssignable);