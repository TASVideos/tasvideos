const mainIvatarDomain = "seccdn.libravatar.org";
const noAvatarImagePath = "/images/empty.png";

let emailBoxElem = document.querySelector('[data-email-box]');

let avatarBoxElem = document.querySelector('[data-avatar-box]');
let ivatarBoxElem = document.getElementById('ivatar-email');

document.addEventListener("DOMContentLoaded", validateAvatar);
document.addEventListener("DOMContentLoaded", onIvatarToggle);
avatarBoxElem.addEventListener('input', generateAvatarPreview);
ivatarBoxElem.addEventListener('input', generateIvatarPreview);
let avatarImgElem = document.getElementById('avatar-img');
avatarImgElem.onload = validateAvatar;
avatarImgElem.onerror = () => {
    avatarImgElem.src = noAvatarImagePath;
};
Array.from(document.querySelectorAll('[name="UseIvatar"]')).forEach(elem => elem.addEventListener('click', onIvatarToggle));

function generateAvatarPreview() {
    const avatar = avatarBoxElem.value;
    if (avatar) {
        avatarImgElem.src = avatar;
    } else {
        avatarImgElem.src = '';
        preventSave(false);
    }
}

function validateAvatar() {
    const maxWidth = 125;
    const maxHeight = 125;
    const descSection = document.getElementById('avatar-description');

    const tooBig = avatarImgElem.width > maxWidth || avatarImgElem.height > maxHeight;
    if (tooBig) {
        descSection.classList.add('text-danger');
        document.getElementById('avatar-too-big').classList.remove('d-none');
        preventSave(true);
    } else {
        descSection.classList.remove('text-danger');
        document.getElementById('avatar-too-big').classList.add('d-none');
        preventSave(false);
    }
}

function preventSave(prevent) {
    document.getElementById('submit-btn').disabled = prevent;
}

function avatarIsIvatar() {
    return avatarBoxElem.value?.includes(`//${mainIvatarDomain}/`);
}

async function onIvatarToggle() {
    const checked = document.querySelector('[name="UseIvatar"]:checked').value == 'True';
    if (checked) {
        document.getElementById('ivatar-section').classList.remove('d-none');
        console.log('profile email value', emailBoxElem.value)
        const profileEmailIvatarValue = await getIvatarUrl(emailBoxElem);
        console.log('Ivatar generated from email value', profileEmailIvatarValue)
        if (avatarIsIvatar() && profileEmailIvatarValue != avatarBoxElem.value) {
            ivatarBoxElem.value = '';
        } else {
            ivatarBoxElem.value = emailBoxElem.value;
        }

        await generateIvatarPreview();
    } else {
        document.getElementById('ivatar-section').classList.add('d-none');
    }
}

async function generateIvatarPreview() {
    const url = await getIvatarUrl(ivatarBoxElem);
    if (url) {
        avatarBoxElem.value = url;
    }

    generateAvatarPreview();
}

async function getIvatarUrl(boxElem) {
    const email = boxElem?.value.toLowerCase();
    if (!email) {
        return;
    }

    const hash = await createSha256(email);
    return `https://${mainIvatarDomain}/avatar/${hash}?d=${noAvatarImagePath}`;
}

async function createSha256(string) {
    const utf8 = new TextEncoder().encode(string);
    const hashBuffer = await crypto.subtle.digest('SHA-256', utf8);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    return hashArray
        .map((bytes) => bytes.toString(16).padStart(2, '0'))
        .join('');
}