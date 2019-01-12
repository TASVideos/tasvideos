// ReSharper disable once NativeTypePrototypeExtending

// Polyfills
(function () {
	if (typeof NodeList.prototype.forEach !== 'function') {
		NodeList.prototype.forEach = Array.prototype.forEach;
	}

	if (typeof Element.prototype.remove !== 'function') {
		Element.prototype.remove = function () {
			if (this.parentNode) {
				this.parentNode.removeChild(this);
			}
		};
	}

	if (typeof window.CustomEvent !== 'function') {
		// ReSharper disable once InconsistentNaming
		function CustomEvent(event, params) {
			params = params || { bubbles: false, cancelable: false, detail: undefined };
			var evt = document.createEvent('CustomEvent');
			evt.initCustomEvent(event, params.bubbles, params.cancelable, params.detail);
			return evt;
		}

		CustomEvent.prototype = window.Event.prototype;
		window.CustomEvent = CustomEvent;
	}

	if (!Element.prototype.closest)
		Element.prototype.closest = function (s) {
			var el = this;
			if (!document.documentElement.contains(el)) return null;
			do {
				if (el.matches(s)) return el;
				el = el.parentElement || el.parentNode;
			} while (el && el.nodeType === 1);
			return null;
		};

	if (typeof Array.prototype.includes !== 'function') {
		Array.prototype.includes = function (searchElement, fromIndex) {
			// 1. Let O be ? ToObject(this value).
			var o = Object(this);

			// 2. Let len be ? ToLength(? Get(O, "length")).
			var len = o.length >>> 0;

			// 3. If len is 0, return false.
			if (len === 0) {
				return false;
			}

			// 4. Let n be ? ToInteger(fromIndex).
			//    (If fromIndex is undefined, this step produces the value 0.)
			var n = fromIndex | 0;

			// 5. If n ≥ 0, then
			//  a. Let k be n.
			// 6. Else n < 0,
			//  a. Let k be len + n.
			//  b. If k < 0, let k be 0.
			var k = Math.max(n >= 0 ? n : len - Math.abs(n), 0);

			function sameValueZero(x, y) {
				return x === y || (typeof x === 'number' && typeof y === 'number' && isNaN(x) && isNaN(y));
			}

			// 7. Repeat, while k < len
			while (k < len) {
				// a. Let elementK be the result of ? Get(O, ! ToString(k)).
				// b. If SameValueZero(searchElement, elementK) is true, return true.
				if (sameValueZero(o[k], searchElement)) {
					return true;
				}
				// c. Increase k by 1. 
				k++;
			}

			// 8. Return false
			return false;
		}
	};
})();

// Helper "extension methods"
NodeList.prototype.toArray = function () {
	return Array.prototype.slice.call(this);
};

// does this go here?
function ajaxModuleHelper(name, params, elementId) {
	var $element = $("script[data-ajaxmoduleid=" + elementId + "]");
	if (!$element.length) { // a previous if module call might have removed this from the DOM
		return;
	}
	var x = new XMLHttpRequest();
	x.onreadystatechange = function() {
		if (x.readyState === XMLHttpRequest.DONE && x.status === 200) {
			if ($.contains(document.body, $element[0])) { // an interceding if module call might have removed this from the DOM
				$element.replaceWith($(x.responseText));
			}
		}
	};

	x.open("GET", "/Wiki/DoModule?Name=" + encodeURIComponent(name) + "&Params=" + encodeURIComponent(params), true);
	x.send();
}

function ajaxIfModuleHelper(condition, elementId) {
	var $element = $("span[data-ajaxmoduleid=" + elementId + "]");
	if (!$element.length) { // a previous if module call might have removed this from the DOM
		return;
	}
	var x = new XMLHttpRequest();
	x.onreadystatechange = function() {
		if (x.readyState === XMLHttpRequest.DONE && x.status === 200) {
			if ($.contains(document.body, $element[0])) { // an interceding if module call might have removed this from the DOM
				if (JSON.parse(x.responseText)) {
					// first child is the script; remove that
					$element.children().first().remove();
					$element.replaceWith($element.children());
				} else {
					$element.remove();
				}
			}
		}
	};

	x.open("GET", "/Wiki/DoIfModule?Condition=" + encodeURIComponent(condition), true);
	x.send();
}
