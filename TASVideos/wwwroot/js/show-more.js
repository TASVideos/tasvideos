document.addEventListener("DOMContentLoaded", findShowMores);

function findShowMores() {
    Array.from(document.querySelectorAll('[data-show-more="true"]')).forEach(elem => {
        registerShowMore(elem);
    });
}

function registerShowMore(content) {
    const reverse = content.dataset.reverse == "true";
    if (reverse) {
        content.scrollTop = 1e10;
    }

    const show = document.getElementById(`show-${content.id}`);
    const hide = document.getElementById(`hide-${content.id}`);
    const height = content.style.maxHeight;
    const clHeight = content.clientHeight;
    content.style.overflowY = 'hidden'
    show.classList.remove('d-none');
    show.onclick = function () {
    	let scroll = content.scrollTop;
    	content.style.overflowY = null;
    	content.style.maxHeight = null;
    	show.classList.add('d-none');
    	hide.classList.remove('d-none');
    	window.scrollBy({ top: scroll, left: 0, behavior: 'instant' });
    }
    hide.onclick = function () {
    	window.scrollBy({ top: clHeight - content.scrollHeight, left: 0, behavior: 'instant' });
    	content.style.overflowY = 'hidden';
    	content.style.maxHeight = height;
    	hide.classList.add('d-none');
    	show.classList.remove('d-none');
        content.scrollTop = reverse ? "1e10" : "0";
    };

    show.querySelector('a').onclick = () => false;
    hide.querySelector('a').onclick = () => false;

    content.onclick = function (e) {
        if (e.target.tagName === 'A' && e.target.getAttribute('href')?.startsWith('#') && content.style.overflowY === 'hidden') {
            show.click();
        }
    }
}
