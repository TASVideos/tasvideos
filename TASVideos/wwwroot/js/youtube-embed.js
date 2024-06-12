const youtubeScriptId = 'youtube-api';
let youtubeScript = document.getElementById(youtubeScriptId);
let player;
// Loads the scripts which run the YouTube Player API asynchronously * @
if (youtubeScript === null) {
	let tag = document.createElement('script');
	let firstScript = document.getElementsByTagName('script')[0];

	tag.src = 'https://www.youtube.com/iframe_api';
	tag.id = youtubeScriptId;
	firstScript.parentNode.insertBefore(tag, firstScript);
}

// Populates the YouTube player after the API script is ready * @
if (!player) {
	window.onYouTubeIframeAPIReady = () => {
		player = new YT.Player('ytplayer', {
			videoId: document.getElementById('ytplayer').dataset.url
		});
	}
}