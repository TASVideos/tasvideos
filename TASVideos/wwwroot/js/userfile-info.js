Array.from(document.querySelectorAll('[data-edit-btn]')).forEach(btn => {
	btn.addEventListener('click', () => {
		const commentId = btn.dataset.commentId;
		btn.classList.add('d-none');
		document.getElementById(`edit-comment-${commentId}`).classList.remove('d-none');
		document.getElementById(`view-comment-${commentId}`).classList.add('d-none');
	});
});

Array.from(document.querySelectorAll('[data-cancel-btn]')).forEach(btn => {
	btn.addEventListener('click', () => {
		const commentId = btn.dataset.commentId;
		document.getElementById(`edit-comment-${commentId}`).classList.add('d-none');
		document.getElementById(`view-comment-${commentId}`).classList.remove('d-none');
		document.getElementById(`edit-button-${commentId}`).classList.remove('d-none');
	});
});