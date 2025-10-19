document.addEventListener("DOMContentLoaded", registerFileInputs);

function registerFileInputs() {
	Array.from(document.querySelectorAll('[data-id="movie-file"]')).forEach(elem => {
		elem.addEventListener("input", async () => {
			await compressInput(elem);
		})
	});
}

async function compressInput(fileInput) {
	const inputFile = fileInput.files[0];
	if (!inputFile) {
		return;
	}

	const inputBytes = await inputFile.arrayBuffer();
	const zippedBytes = pako.gzip(inputBytes);

	const newFile = new File([zippedBytes], inputFile.name, { type: inputFile.type, lastModified: inputFile.lastModified });
	const dto = new DataTransfer;
	dto.items.add(newFile);
	fileInput.files = dto.files;
}