document.addEventListener("DOMContentLoaded", registerFileInputs);

function registerFileInputs() {
	const noZipReminder = document.querySelector('[data-id="nozip-reminder"]');
	Array.from(document.querySelectorAll('[data-id="movie-file"]')).forEach(elem => {
		elem.addEventListener("input", async () => {
			await compressInput(elem, noZipReminder);
		})
	});
}

async function compressInput(fileInput, noZipReminder) {
	const inputFile = fileInput.files[0];
	if (!inputFile) {
		return;
	}

	if (noZipReminder) {
		const ext = (s => s.substring(s.lastIndexOf('.') + 1))(inputFile.name);
		if (ext.toLowerCase() === "zip") {
			noZipReminder.classList.remove('d-none');
		}
		else {
			noZipReminder.classList.add('d-none');
		}
	}

	const inputBytes = await inputFile.arrayBuffer();
	const zippedBytes = pako.gzip(inputBytes);

	const newFile = new File([zippedBytes], inputFile.name, { type: inputFile.type, lastModified: inputFile.lastModified });
	const dto = new DataTransfer;
	dto.items.add(newFile);
	fileInput.files = dto.files;
}