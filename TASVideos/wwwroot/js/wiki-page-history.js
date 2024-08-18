window.addEventListener('DOMContentLoaded', function () {
	if (document.getElementById('has-diff')) {
		generateDiff();
	}
});

Array.from(document.querySelectorAll('[name="diff-type"]')).forEach(dt => {
	dt.addEventListener('click', generateDiff);
});

Array.from(document.querySelectorAll('[name="context-size"]')).forEach(c => {
	c.addEventListener('input', generateDiff);
});

Array.from(document.querySelectorAll('[data-from]')).forEach(btn => {
	const revision = btn.dataset.revision;
	btn.addEventListener('click', () => {
		diffBtnClicked(revision, null);
	});
});

Array.from(document.querySelectorAll('[data-to]')).forEach(btn => {
	const revision = btn.dataset.revision;
	btn.addEventListener('click', () => {
		diffBtnClicked(null, revision);
	});
});

let fromRevision, toRevision;
function diffBtnClicked(from, to) {
	const path = document.querySelector('[data-id="path"]').value
	if (from) {
		fromRevision = from;
	}

	if (to) {
		toRevision = to;
	}

	if (fromRevision && toRevision) {
		window.location = `/Wiki/PageHistory?path=${encodeURIComponent(path)}&fromRevision=${fromRevision}&toRevision=${toRevision}`;
	} else {
		updateTableStyling();
	}
}

function updateTableStyling() {
	Array.from(document.querySelectorAll('tbody[data-hasrevisions] tr'))
		.forEach(function (elem) {
			elem.querySelector("button[data-from]").classList.remove("bg-warning");
			elem.querySelector("button[data-to]").classList.remove("bg-warning");
		});

	const cur = document.querySelector(`tr[data-revision="${toRevision}"]`);
	if (cur) {
		cur.querySelector("button[data-to]").classList.add("bg-warning");
	}

	const prev = document.querySelector(`tr[data-revision="${fromRevision}"]`);
	if (prev) {
		prev.querySelector("button[data-from]").classList.add("bg-warning");
	}
}