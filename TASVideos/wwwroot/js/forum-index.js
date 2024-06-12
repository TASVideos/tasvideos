document.getElementById('mark-all-posts').addEventListener('click', markAllPostsRead);
let currentTime = Number(document.getElementById('serverUnixTime').textContent);
{
	let visitedPosts = localStorage.getItem('VisitedPosts');
	visitedPosts = JSON.parse(visitedPosts) ?? {};
	let activitySubforums = document.getElementsByClassName('activity-subforum');
	for (let activitySubforum of activitySubforums) {
		let displayClass;
		let firstPostId, firstPostDate;
		let activityPostsCreated = activitySubforum.dataset.activityPostsCreated ? JSON.parse(activitySubforum.dataset.activityPostsCreated) : {};
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
			let activityPostsEdited = activitySubforum.dataset.activityPostsEdited ? JSON.parse(activitySubforum.dataset.activityPostsEdited) : {};
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
			activitySubforum.classList.add(displayClass);
			activitySubforum.classList.remove('d-none');
			activitySubforum.parentElement.setAttribute('href', `/Forum/Posts/${firstPostId}`);
		}
	}

	// clean up localStorage
	let allActivityPostIds = new Set();
	for (let activitySubforum of activitySubforums) {
		let activityPostsCreated = activitySubforum.dataset.activityPostsCreated ? JSON.parse(activitySubforum.dataset.activityPostsCreated) : {};
		for (let postId in activityPostsCreated) {
			allActivityPostIds.add(postId);
		}
		let activityPostsEdited = activitySubforum.dataset.activityPostsEdited ? JSON.parse(activitySubforum.dataset.activityPostsEdited) : {};
		for (let postId in activityPostsEdited) {
			allActivityPostIds.add(postId);
		}
	}
	Object.keys(visitedPosts).filter(postId => !allActivityPostIds.has(postId)).forEach(postId => delete visitedPosts[postId]);
	localStorage.setItem('VisitedPosts', JSON.stringify(visitedPosts));
}

function markAllPostsRead() {
	let visitedPosts = localStorage.getItem('VisitedPosts');
	visitedPosts = JSON.parse(visitedPosts) ?? {};
	let activitySubforums = document.getElementsByClassName('activity-subforum');
	for (let activitySubforum of activitySubforums) {
		let activityPostsCreated = activitySubforum.dataset.activityPostsCreated ? JSON.parse(activitySubforum.dataset.activityPostsCreated) : {};
		for (let postId in activityPostsCreated) {
			visitedPosts[postId] = currentTime;
		}
		let activityPostsEdited = activitySubforum.dataset.activityPostsEdited ? JSON.parse(activitySubforum.dataset.activityPostsEdited) : {};
		for (let postId in activityPostsEdited) {
			visitedPosts[postId] = currentTime;
		}
	}
	localStorage.setItem('VisitedPosts', JSON.stringify(visitedPosts));
	location.reload();
}