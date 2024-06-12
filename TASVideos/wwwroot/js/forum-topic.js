{
	let currentTime = Number(document.getElementById('serverUnixTime').textContent);
	let activityPosts = document.getElementsByClassName('activity-post');
	let visitedPosts = localStorage.getItem('VisitedPosts');
	visitedPosts = JSON.parse(visitedPosts) ?? {};
	for (let i = 0; i < activityPosts.length; i++) {
		let lastVisit = visitedPosts[activityPosts[i].dataset.postId];
		if (activityPosts[i].dataset.unixCreated && (lastVisit === undefined || lastVisit < activityPosts[i].dataset.unixCreated)) {
			activityPosts[i].classList.add('text-warning');
		} else if (activityPosts[i].dataset.unixEdited && (lastVisit === undefined || lastVisit < activityPosts[i].dataset.unixEdited)) {
			activityPosts[i].classList.add('text-info');
		}
		visitedPosts[activityPosts[i].dataset.postId] = currentTime;
	}
	localStorage.setItem('VisitedPosts', JSON.stringify(visitedPosts));
}