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

// Global browser detection
var isIE = function () {
	return window.navigator.userAgent.indexOf('MSIE') > 0
		|| window.navigator.userAgent.indexOf('Trident') > 0;
}

if (typeof NodeList.toArray !== 'function') {
	NodeList.prototype.toArray = function () {
		return Array.prototype.slice.call(this);
	}
}