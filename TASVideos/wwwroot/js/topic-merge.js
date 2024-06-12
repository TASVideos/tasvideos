const forumModel = document.querySelector('[data-id="destination-forum-id"]');
const topicModel = document.querySelector('[data-id="destination-topic-id"]');
const id = document.getElementById('Id').value;
forumModel.onchange = function () {
	const url = `/Forum/Topics/Merge/${id}?handler=TopicsForForum&forumId=${this.value}`;
	fetch(url).then(r => r.text()).then(t => topicModel.innerHTML = t);
}