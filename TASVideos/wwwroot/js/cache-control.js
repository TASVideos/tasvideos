const getCacheBtn = document.getElementById('get-cache');
const getCacheBox = document.getElementById('get-cache-box');
const cacheValueBox = document.getElementById('get-cache-valuebox');
getCacheBtn.onclick = function() {
	fetch(`/Diagnostics/CacheControl?handler=CacheValue&key=${getCacheBox.value}`)
		.then(handleFetchErrors)
		.then(r => r.json())
		.then(json => {
			cacheValueBox.value = JSON.stringify(json.value);
		});
}