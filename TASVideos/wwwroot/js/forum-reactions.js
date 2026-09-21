window.addEventListener('DOMContentLoaded', function () {
	const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

	function registerReactionBar(bar) {
		if (bar.dataset.isLocked === "True") {
			return;
		}

		Array.from(bar.querySelectorAll('[data-id="reaction-toggle"]')).forEach(toggle => {
			toggle.addEventListener('click', function(e) {
				const choices = bar.querySelector('[data-id="reaction-choices"]');
				choices.classList.toggle('d-none');
			});
		});

		Array.from(bar.querySelectorAll('[data-id="reaction-btn"]')).forEach(btn => {
			btn.addEventListener('click', async function(e) {
				const reaction = btn.dataset.reaction;
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

		Array.from(bar.querySelectorAll('[data-id="reaction-display"]')).forEach(btn => {
			btn.addEventListener('click', async function(e) {
				const reaction = btn.dataset.reaction;
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

	Array.from(document.querySelectorAll('[data-id="reaction-bar"]')).forEach(bar => {
		registerReactionBar(bar);
	});

	async function updateReactionBar(response, bar) {
		if (response.ok) {
			var newHtml = await response.text();

			const template = document.createElement("template");
			template.innerHTML = newHtml;
			const newBar = template.content.firstElementChild;
			bar.replaceWith(newBar);
			registerReactionBar(newBar);
		}
	}
});
