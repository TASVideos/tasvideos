document.getElementById('mark-all-posts').addEventListener('click', markSubforumPostsRead);
let currentTime = Number(document.getElementById('serverUnixTime').textContent);
{
	let visitedPosts = localStorage.getItem('VisitedPosts');
	visitedPosts = JSON.parse(visitedPosts) ?? {};
	let activityTopics = document.getElementsByClassName('activity-topic');
	for (let activityTopic of activityTopics) {
		let displayClass;
		let firstPostId, firstPostDate;
		let activityPostsCreated = activityTopic.dataset.activityPostsCreated ? JSON.parse(activityTopic.dataset.activityPostsCreated) : {};
		for (let postId in activityPostsCreated) {
			let lastVisit = visitedPosts[postId];
			if (!lastVisit || lastVisit < activityPostsCreated[postId]) {
				displayClass = 'text-warning';
				if (!firstPostId || activityPostsCreated[postId] < firstPostDate) {
					firstPostId = postId;
					firstPostDate = activityPostsCreated[postId];
				}
			}
		}
		if (!displayClass) {
			let activityPostsEdited = activityTopic.dataset.activityPostsEdited ? JSON.parse(activityTopic.dataset.activityPostsEdited) : {};
			for (let postId in activityPostsEdited) {
				let lastVisit = visitedPosts[postId];
				if (!lastVisit || lastVisit < activityPostsEdited[postId]) {
					displayClass = 'text-info';
					if (!firstPostId || activityPostsEdited[postId] < firstPostDate) {
						firstPostId = postId;
						firstPostDate = activityPostsEdited[postId];
					}
				}
			}
		}
		if (displayClass) {
			activityTopic.classList.add(displayClass);
			activityTopic.classList.remove('d-none');
			activityTopic.parentElement.setAttribute('href', `/Forum/Posts/${firstPostId}`);
		}
	}
}
function markSubforumPostsRead(){
	let visitedPosts = localStorage.getItem('VisitedPosts');
	visitedPosts = JSON.parse(visitedPosts) ?? {};
	const activityTopics = document.getElementsByClassName('activity-topic');
	for (let activityTopic of activityTopics) {
		const activityPostsCreated = activityTopic.dataset.activityPostsCreated ? JSON.parse(activityTopic.dataset.activityPostsCreated) : {};
		for (let postId in activityPostsCreated) {
			visitedPosts[postId] = currentTime;
		}
		const activityPostsEdited = activityTopic.dataset.activityPostsEdited ? JSON.parse(activityTopic.dataset.activityPostsEdited) : {};
		for (let postId in activityPostsEdited) {
			visitedPosts[postId] = currentTime;
		}
	}
	localStorage.setItem('VisitedPosts', JSON.stringify(visitedPosts));
	location.reload();
}