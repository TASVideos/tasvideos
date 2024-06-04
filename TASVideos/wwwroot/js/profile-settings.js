let emailBoxElem = document.querySelector('[data-email-box]');

let avatarBoxElem = document.querySelector('[data-avatar-box]');
let gravatarBoxElem = document.getElementById('gravatar-email');

document.addEventListener("DOMContentLoaded", validateAvatar);
document.addEventListener("DOMContentLoaded", onGravatarToggle);
avatarBoxElem.addEventListener('input', generateAvatarPreview);
document.getElementById('gravatar-email').addEventListener('input', generateGravatarPreview)
let avatarImgElem = document.getElementById('avatar-img');
avatarImgElem.onload = validateAvatar;

Array.from(document.querySelectorAll('[name="UseGravatar"]')).forEach(elem => elem.addEventListener('click', onGravatarToggle));

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

function avatarIsGravatar() {
    return avatarBoxElem.value?.includes('gravatar');
}

async function onGravatarToggle() {
    const checked = document.querySelector('[name="UseGravatar"]:checked').value == 'True';
    if (checked) {
        document.getElementById('gravatar-section').classList.remove('d-none');
        console.log('profile email value', emailBoxElem.value)
        const profileEmailGravatarValue = await getGravatarUrl(emailBoxElem);
        console.log('gravatar generated from email value', profileEmailGravatarValue)
        if (avatarIsGravatar() && profileEmailGravatarValue != avatarBoxElem.value) {
            gravatarBoxElem.value = '';
        } else {
            gravatarBoxElem.value = emailBoxElem.value;
        }

        await generateGravatarPreview();
    } else {
        document.getElementById('gravatar-section').classList.add('d-none');
    }
}

async function generateGravatarPreview() {
    const url = await getGravatarUrl(gravatarBoxElem);
    if (url) {
        avatarBoxElem.value = url;
    }

    generateAvatarPreview();
}

async function getGravatarUrl(boxElem) {
    const email = boxElem?.value.toLowerCase();
    if (!email) {
        return;
    }

    const hash = await createSha256(email);
    return `https://gravatar.com/avatar/${hash}`;
}

async function createSha256(string) {
    const utf8 = new TextEncoder().encode(string);
    const hashBuffer = await crypto.subtle.digest('SHA-256', utf8);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    return hashArray
        .map((bytes) => bytes.toString(16).padStart(2, '0'))
        .join('');
}