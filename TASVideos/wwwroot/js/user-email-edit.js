"use strict";
const emailConfirmedBox = document.querySelector('[data-id="email-confirmed"]');
const email = document.querySelector('[data-id="email"]');
email.addEventListener('input', () => { emailConfirmedBox.checked = false });