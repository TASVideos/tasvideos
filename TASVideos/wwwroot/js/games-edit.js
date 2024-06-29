let dropArea = document.getElementById('hash-drop-area');
let hashInput = document.getElementById('hash-input');
let hashProgress = document.getElementById('hash-progress');
let sha1Input = document.querySelector('[data-id="sha1"]');
let md5Input = document.querySelector('[data-id="md5"]');
let nameInput = document.querySelector('[data-id="name"]');
function dropEnterStyle() {
	dropArea.classList.add('border-secondary');
	dropArea.classList.remove('border-silver');
}
function dropLeaveStyle() {
	dropArea.classList.add('border-silver');
	dropArea.classList.remove('border-secondary');
}
function calculateHashes(file) {
	nameInput.value = file.name;
	const sha1 = CryptoJS.algo.SHA1.create();
	const md5 = CryptoJS.algo.MD5.create();
	const fileSize = file.size;
	const chunkSize = 16 * 1024 * 1024;
	let offset = 0;

	if (fileSize > chunkSize) {
		hashProgress.style.width = '0%';
		hashProgress.parentElement.classList.remove('d-none');
	}

	const reader = new FileReader();
	reader.onload = function () {
		offset += reader.result.length;
		sha1.update(CryptoJS.enc.Latin1.parse(reader.result));
		md5.update(CryptoJS.enc.Latin1.parse(reader.result));
		hashProgress.style.width = `${Math.ceil((offset / fileSize) * 100)}%`;
		if (offset >= fileSize) {
			const sha1Hash = sha1.finalize();
			const md5Hash = md5.finalize();
			sha1Input.value = sha1Hash.toString(CryptoJS.enc.Hex);
			md5Input.value = md5Hash.toString(CryptoJS.enc.Hex);
			setTimeout(() => { hashProgress.parentElement.classList.add('d-none'); }, 600);
			return;
		}
		readNext();
	};

	function readNext() {
		const fileSlice = file.slice(offset, offset + chunkSize);
		reader.readAsBinaryString(fileSlice);
	}

	readNext();
}
hashInput.addEventListener('change', (e) => {
	calculateHashes(e.currentTarget.files[0]);
});
dropArea.addEventListener('click', () => {
	hashInput.click();
});
dropArea.addEventListener('dragover', (e) => {
	e.preventDefault();
});
let dropCount = 0;
dropArea.addEventListener('dragenter', () => {
	if (dropCount == 0) {
		dropEnterStyle();
	}
	dropCount++;
});
dropArea.addEventListener('dragleave', () => {
	dropCount--;
	if (dropCount == 0) {
		dropLeaveStyle();
	}
});
dropArea.addEventListener('drop', (e) => {
	e.preventDefault();
	dropLeaveStyle();
	dropCount = 0;
	if (e.dataTransfer.items) {
		for (let item of e.dataTransfer.items) {
			if (item.kind == 'file') {
				calculateHashes(item.getAsFile());
				break;
			}
		}
	}
});