"use strict";
{
	const fileInput = document.querySelector('[data-id="user-form-file"]');
	const storageAvailable = parseFloat(document.getElementById("StorageAvailable").value);
	const supportedFileExtensions = fileInput.dataset.supportedExtensions.split(',');
	const fileInputWarning = document.getElementById("FileInputWarning");
	fileInput.addEventListener("input", async () => {
		const inputFile = fileInput.files[0];
		if (!inputFile) {
			return;
		}

		const ext = (s => s.substring(s.lastIndexOf('.') + 1))(fileInput.files[0].name);
		if (ext) {
			if (supportedFileExtensions.includes(ext)) {
				fileInputWarning.innerText = "";
			} else {
				fileInput.value = "";
				fileInputWarning.innerText = `Invalid file extension: ${ext}`;
				return;
			}
		}

		const inputBytes = await inputFile.arrayBuffer();
		const zippedBytes = pako.gzip(inputBytes);

		const newFile = new File([zippedBytes], inputFile.name, { type: inputFile.type, lastModified: inputFile.lastModified });
		if (newFile.size < storageAvailable) {
			const dto = new DataTransfer;
			dto.items.add(newFile);
			fileInput.files = dto.files;
			fileInputWarning.innerText = "";
		} else {
			fileInput.value = "";
			fileInputWarning.innerText = "Chosen File Exceeds Quota";
		}
	});
}