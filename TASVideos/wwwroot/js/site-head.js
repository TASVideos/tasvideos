
const getStoredTheme = () => localStorage.getItem('theme');
const setStoredTheme = theme => localStorage.setItem('theme', theme);

const getPreferredTheme = () => {
    const storedTheme = getStoredTheme();
    if (storedTheme) {
        return storedTheme;
    }
    const oldStoredTheme = localStorage.getItem('style-dark');
    if (oldStoredTheme) {
        localStorage.removeItem('style-dark');
        if (oldStoredTheme === 'true') {
            return 'dark';
        }
        else if (oldStoredTheme === 'false') {
            return 'light';
        }
    }

    return 'auto';
}

const setTheme = theme => {
    if (theme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        document.documentElement.setAttribute('data-bs-theme', 'dark');
    } else {
        document.documentElement.setAttribute('data-bs-theme', theme);
    }
    setStoredTheme(theme);
}

setTheme(getPreferredTheme());

window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
    const storedTheme = getStoredTheme();
    if (storedTheme !== 'light' && storedTheme !== 'dark') {
        setTheme(getPreferredTheme());
    }
});

window.addEventListener("DOMContentLoaded", function () {
    Array.from(document.querySelectorAll('[data-theme]')).forEach(btn => {
        btn.addEventListener('click', () => {
            setTheme(btn.dataset.theme);
        });
    });

    Array.from(document.querySelectorAll('a[data-rate-btn]')).forEach(a => {
        a.onclick = () => {
            toggleDisplay(a.dataset.pubId);
            return false;
        };
    });

    Array.from(document.querySelectorAll('button[data-save-rating-btn]')).forEach(btn => {
        btn.onclick = () => {
            rate(btn.dataset.pubId, false);
        };
    });

    Array.from(document.querySelectorAll('button[data-remove-rating-btn]')).forEach(btn => {
        btn.onclick = () => {
            rate(btn.dataset.pubId, true);
        };
    });

    const searchBtnElem = document.querySelector('form[action="/Search/Index"] a');
    searchBtnElem.addEventListener('click', e => {
        e.preventDefault();
        e.target.closest("form").requestSubmit();
    });
});

function rate(pubId, unrated) {
    const rating = unrated ? null : document.querySelector(`#rate-${pubId} #Rating_Rating`).value;
    fetch(`/Publications/Rate/${pubId}?handler=Inline`, {
        method: 'POST',
        body: JSON.stringify(rating),
        headers: {
            RequestVerificationToken:
                document.getElementById('RequestVerificationToken').value
        }
    })
        .then(r => {
            if (!r.ok) {
                throw Error(r.statusText);
            }
            return r;
        })
        .then(r => r.text())
        .then(r => {
            const update = JSON.parse(r);
            if (!rating) {
                document.querySelector(`#ownRating-${pubId}`).innerHTML = 'Rate';
            } else {
                document.querySelector(`#ownRating-${pubId}`).innerHTML = `Rated ${rating} / 10`;
            }
            document.querySelector(`#overallRating-${pubId}`).innerHTML = update.overallRating;
            toggleDisplay(pubId);
        })
        .catch(() => alert("Rating failed"));
}

function toggleDisplay(pubId) {
    ratingConnect(document.querySelector(`#rate-${pubId} #Rating_Rating`), document.querySelector(`#slider-${pubId}`));
    document.querySelector(`#rate-${pubId}`).classList.toggle('d-none');
}

function ratingConnect(textbox, slider) {
    textbox.oninput = function () {
        slider.value = textbox.value;
    };
    slider.oninput = function () {
        slider.value = Math.round(Number(this.value) * 2) / 2;
        textbox.value = slider.value;
    };
}
