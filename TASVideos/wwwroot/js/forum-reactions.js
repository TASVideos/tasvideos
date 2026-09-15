window.addEventListener('DOMContentLoaded', function () {
	const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
	function registerReactionBars(parent) {
		Array.from(parent.querySelectorAll('[data-id="reaction-toggle"]')).forEach(toggle => {
			console.log(toggle);
			toggle.addEventListener('click', function (e) {
				const choices = toggle.closest('[data-id="reaction-bar"]').querySelector('[data-id="reaction-choices"]');
				choices.classList.toggle('d-none');
			});
		});

		Array.from(parent.querySelectorAll('[data-id="reaction-btn"]')).forEach(btn => {
			btn.addEventListener('click', async function (e) {
				const reaction = btn.dataset.reaction;
				const bar = btn.closest('[data-id="reaction-bar"]');
				const postId = bar.dataset.postId;

				var response = await fetch(`/Forum/Posts/${postId}/Reactions`, {
					method: 'POST',
					body: JSON.stringify({ "Reaction": reaction }),
					headers: {
						'Content-Type': 'application/json',
						'RequestVerificationToken': token,
					}
				});
				await updateReactionBar(response, bar);
			});
		});

		Array.from(parent.querySelectorAll('[data-id="reaction-display"]')).forEach(btn => {
			btn.addEventListener('click', async function (e) {
				const reaction = btn.dataset.reaction;
				const bar = btn.closest('[data-id="reaction-bar"]');
				const postId = bar.dataset.postId;
				const isOwnReaction = bar.dataset.ownReaction === reaction;
				const body = isOwnReaction ? null : JSON.stringify({ "Reaction": reaction });

				var response = await fetch(`/Forum/Posts/${postId}/Reactions`, {
					method: 'POST',
					body: body,
					headers: {
						'Content-Type': 'application/json',
						'RequestVerificationToken': token,
					}
				});
				await updateReactionBar(response, bar);
			});
		});
	}

	registerReactionBars(document);

	async function updateReactionBar(response, bar) {
		if (response.ok) {
			var newHtml = await response.text();

			const template = document.createElement("template");
			template.innerHTML = newHtml;
			const newBar = template.content.firstElementChild;
			bar.replaceWith(newBar);
			registerReactionBars(newBar);
		}
	}
});
