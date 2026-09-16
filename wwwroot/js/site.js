// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const TIMER_DURATION_MS = 30 * 60 * 1000;

function formatTimer(ms) {
    const totalSeconds = Math.max(0, Math.ceil(ms / 1000));
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;

    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
}

function updateCountdownTimer() {
    const timerElements = document.querySelectorAll('[data-timer="countdown"]');

    if (!timerElements.length) {
        return;
    }

    const body = document.body;
    let endTime = Number(body.dataset.timerEnd || 0);

    if (!endTime) {
        return;
    }

    const remainingMs = Math.max(0, endTime - Date.now());

    timerElements.forEach((element) => {
        element.textContent = formatTimer(remainingMs);
    });

    if (remainingMs === 0) {
        timerElements.forEach((element) => {
            element.textContent = '00:00';
        });

        if (!body.dataset.timerExpired) {
            body.dataset.timerExpired = 'true';
            window.location.href = '/Home/TiempoAgotado';
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    updateCountdownTimer();
    setInterval(updateCountdownTimer, 1000);
});
