window.addEventListener("DOMContentLoaded", function () {
    const images = document.querySelectorAll('img');
    Array.from(images).forEach(image => {
        image.addEventListener('error',() => {
            image.src = '/images/empty.png';
        });
    });
});
