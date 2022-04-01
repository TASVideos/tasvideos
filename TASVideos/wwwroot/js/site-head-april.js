function forceDarkMode() {
	const darkModeStylesheet = document.getElementById("style-dark");
	if (!darkModeStylesheet) {
		removeAutoDarkMode();

		const newElement = document.createElement("link");
		newElement.rel = "stylesheet";
		newElement.id = "style-dark";
		newElement.href = "/css/darkmode.css";
		document.head.appendChild(newElement);

		localStorage.setItem("style-dark", "true");
	}
}

function forceLightMode() {
	removeForcedDarkMode();
	removeAutoDarkMode();

	localStorage.setItem("style-dark", "false");
}

function autoDarkMode() {
	const initialDarkModeStylesheet = document.getElementById("style-dark-initial");
	if (!initialDarkModeStylesheet) {
		removeForcedDarkMode();

		const newElement = document.createElement("link");
		newElement.rel = "stylesheet";
		newElement.id = "style-dark-initial";
		newElement.href = "/css/darkmode-initial.css";
		document.head.appendChild(newElement);

		localStorage.removeItem("style-dark");
	}
}

function removeForcedDarkMode() {
	const darkModeStylesheet = document.getElementById("style-dark");
	if (darkModeStylesheet) {
		darkModeStylesheet.parentElement.removeChild(darkModeStylesheet);
	}
}

function removeAutoDarkMode() {
	const initialDarkModeStylesheet = document.getElementById("style-dark-initial");
	if (initialDarkModeStylesheet) {
		initialDarkModeStylesheet.parentElement.removeChild(initialDarkModeStylesheet);
	}
}

if (localStorage.getItem("style-dark") !== null) {
	removeAutoDarkMode();
	if (localStorage.getItem("style-dark") === "true") {
		forceDarkMode();
	}
}

let displayRefreshRate = 60;

/**
 * Allows to obtain the estimated Hz of the primary monitor in the system.
 * 
 * @param {Function} callback The function triggered after obtaining the estimated Hz of the monitor.
 * @param {Boolean} runIndefinitely If set to true, the callback will be triggered indefinitely (for live counter).
 */
function getScreenRefreshRate(callback, runIndefinitely) {
    let requestId = null;
    let callbackTriggered = false;
    runIndefinitely = runIndefinitely || false;

    if (!window.requestAnimationFrame) {
        window.requestAnimationFrame = window.mozRequestAnimationFrame || window.webkitRequestAnimationFrame;
    }

    let DOMHighResTimeStampCollection = [];

    let triggerAnimation = function (DOMHighResTimeStamp) {
        DOMHighResTimeStampCollection.unshift(DOMHighResTimeStamp);

        if (DOMHighResTimeStampCollection.length > 10) {
            let t0 = DOMHighResTimeStampCollection.pop();
            let fps = Math.floor(1000 * 10 / (DOMHighResTimeStamp - t0));

            if (!callbackTriggered) {
                callback.call(undefined, fps, DOMHighResTimeStampCollection);
            }

            if (runIndefinitely) {
                callbackTriggered = false;
            } else {
                callbackTriggered = true;
            }
        }

        requestId = window.requestAnimationFrame(triggerAnimation);
    };

    window.requestAnimationFrame(triggerAnimation);

    // Stop after half second if it shouldn't run indefinitely
    if (!runIndefinitely) {
        window.setTimeout(function () {
            window.cancelAnimationFrame(requestId);
            requestId = null;
        }, 500);
    }
}

function observer_callback(list, observer) {
	// After 500ms, will output the estimated Hz of the monitor (frames per second - FPS)
	// 239 FPS (in my case)
	getScreenRefreshRate(function (FPS) {
		displayRefreshRate = FPS;
		const paintTimings = performance.getEntriesByType('paint');
		const fmp = paintTimings.find(({ name }) => name === "first-contentful-paint");

		const message = `Loading this page cost you ${Math.ceil(fmp.startTime / 1000 * displayRefreshRate)} frames`;

		const node = document.createElement("div");
		const textnode = document.createTextNode(message);
		node.appendChild(textnode);
		node.style = "color: RGBA(255,255,255,.6)";

		document.getElementById("banner").appendChild(node);
	});
}

let observer = new PerformanceObserver(observer_callback);
observer.observe({ entryTypes: ["paint"] });
