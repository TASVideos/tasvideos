function showHideScrollToTop() {
	if (window.scrollY > 20) {
		document.getElementById("button-scrolltop").classList.remove("d-none");
	} else {
		document.getElementById("button-scrolltop").classList.add("d-none");
	}
}

function scrollToTop() {
	window.scroll({
		top: 0,
		behavior: 'smooth'
	});
}

function clearDropdown(elemId) {
	Array.from(document.querySelectorAll(`#${elemId} option`))
		.forEach(element => {
			if (element.value) {
				element.remove();
			}
		});
}

function handleFetchErrors(response) {
	if(!response.ok) {
		throw Error(response.statusText);
	}

	return response;
}

window.addEventListener("scroll", showHideScrollToTop);
document.getElementById("button-scrolltop").addEventListener("click", scrollToTop);

if (location.hash) {
	let expandButton = document.querySelector(`[data-bs-target='${location.hash}']`);
	if (expandButton) {
		expandButton.classList.remove("collapsed");
		expandButton.setAttribute("aria-expanded", "true");
	}
	if (isNaN(parseInt(location.hash.substr(1)))) {
		let expandedContent = document.querySelector(location.hash + '.collapse');
		if (expandedContent) {
			expandedContent.classList.add("show");
		}
	}
}
function dom_tag($) { return document.createElement($) } var ratingbarimage = "/images/home.png"; function dom_spimg($, t, e, n, r, o, a) { return $.setAttribute("style", "width:" + e + "px;height:" + n + "px;position:relative;z-index:" + a + ";margin-right:" + -e + "px;margin-bottom:" + -n + "px;background:url(" + t + ") no-repeat " + -r + "px " + -o + "px transparent"), $ } var petx = -50, pety = 0, petescape = !1, petfast = !1, mf = function () { petfast = !0 }, petmode = -1; function MakeJC($, t) { var e, n = [], r = 4 * $ / t, o = 2 * r / t; for (e = 0; e < t; ++e)n.push(Math.floor(r * e - (o * e * e - 1) / 2)); return n } function asx($, t, e) { $.style.left = t + "px", $.style.top = e + "px" } function ra_im($, t, e, n, r, o) { return dom_spimg($, ratingbarimage, t, e, n, r, o) } function RunFireBall($, t, e, n) { var r, o, a, _, i = ra_im(dom_tag("div"), 8, 8, 56, n, 70), p = 9, m = 0, s = 320 == n ? 4 : 2, d = [[0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 3], [3, 0, 1, 4, 2, 0, 1, 2], [5, 6, 7, 8]]; function f($, t) { asx(i, $, t) } function u($) { i.style.backgroundPosition = -(56 + 8 * $) + "px -" + n + "px" } function c() { p > 2 && (_ = 1, a = 1 + 3 * Math.random() / s, o = 5, .5 > Math.random() && (a = -a), f(r = 20 + Math.random() * (t - 40), o), u(d[p = 0][m = 0])), ++m >= d[p].length && (0 == p ? p = 2 : 1 == p && f(r = -50, p = 9), m = 0), p < 9 && (u(d[p][m]), 2 == p && (f(r += a, o += _), (o >= e - 8 || o <= 4) && (_ = -_), (r < 20 || r > t - 20 || petx < r + 8 && petx + 16 >= r && pety < o + 8 && pety + 24 >= o) && (p = 1, m = 0))); var $ = 80; p > 2 && ($ = 5e3 + 3e5 * Math.random()), 2 == p && ($ /= s), setTimeout(c, $) } $.insertBefore(i, $.firstChild), f(r = -50, 0), setTimeout(c, 2e3 + 3e5 * Math.random()) } function RunPet($, t, e, n, r) { var o, a = ra_im(dom_tag("div"), 16, 24, 128, n, 70), _ = 1, i = 0, p = 0, m = 0, s = [2, 0, 4, 6, 5, 6, 3, 1, 7, 9, 8, 9], d = t + 60, f = MakeJC(r, 27), u = -1, c = function () { var n, r = ra_im(dom_tag("div"), 116, 16, 528, 304, 70), o = t + 10, a = 0, _ = 0; function i($, t) { asx(r, $, t) } return $.insertBefore(r, $.firstChild), i(n = -118, 0), function () { if (++_ >= 4) { var $; _ = 0, ++a >= 4 && (a = 0), $ = a, r.style.backgroundPosition = "-528px " + -(304 + 16 * $) + "px" } return i(petx = n += 2, pety = e - 16), n > o && (n = -118, petmode = -1), 35 } }(), x = function () { var n, r, o, a = ra_im(dom_tag("div"), 16, 16, 288, 336, 70), _ = t + 10, i = 0, p = 0, m = 0, s = [[104, 120, 40, 80], [104, 80, 40, 120], [0, 16, 40, 56], [0, 56, 40, 16]]; function d($, t, e) { var n = 16, r = 288 + $, o = 336 + m; (16 == $ || 120 == $) && (n = 24, t -= 3), (56 == $ || 80 == $) && (n = 24, t -= 6), a.style.backgroundPosition = -r + "px -" + o + "px", a.style.marginRight = -r + "px", a.style.width = n + "px", asx(a, t, e) } return $.insertBefore(a, $.firstChild), d(0, n = -26, 0), function () { return -26 == n && (m = .5 > Math.random() ? 0 : 16, o = s[Math.floor(4 * Math.random())], .5 > Math.random() ? (n = -24, r = 9) : (n = t, r = -9)), ++p >= 3 && (p = 0, ++i >= o.length && (i = 0)), d(o[i], petx = n += r, pety = e - 16), (n > _ || n < -24) && (n = -26, petmode = -1), 25 } }(); function g($, t) { asx(a, $, t) } function l() { return o <= -20 || o > d } function v() { if (petescape && !l() && 25 != p && -25 != p && ((p = o < t / 2 ? -25 : 25) > 0 && -1 == _ && (i = -i), m = 0), -1 == petmode && (petmode = Math.random()), petmode < .01 && (petfast = !0), petmode < .05) return setTimeout(v, c()); if (petmode < .15) return setTimeout(v, x()); u >= 0 && ++u >= f.length && (u = -1), -1 == u && i != p && (i < p ? ++i : --i); var $, h = 0, y = i < 0 ? -i : i; if (y <= 1 && petescape && (_ = 1, p < 0 && (_ = -1, p = -p)), y > 0) h = s[3 + 3 * _ + (u >= 0 ? 1 : 2 + (m = (m + 1) % 4))], g(petx = o += _ * ((i < 0 ? -i : i) / 4), pety = e - 24 - (u < 0 ? 0 : f[u])), (!petescape && 0 == m && .1 > Math.random() || o <= -20 && -1 == _ || o > d && 1 == _) && (p = 0), l() && (petescape = !1, m = 0), !petescape && 0 == m && o >= 60 && o < 260 && r > 0 && -1 == u && .1 > Math.random() && (u = 0); else { h = s[3 + 3 * _]; var b = .05; (petfast || o >= 0 && o < t) && (b = .5), !petescape && 0 == ++m && Math.random() < b && (_ = o <= -20 ? 1 : o >= d ? -1 : .5 > Math.random() ? -1 : 1, p = 7, m = 0), m >= 20 && (m = -2) } $ = h, a.style.backgroundPosition = -(128 + 16 * $) + "px -" + n + "px"; var k = 35; !petfast && l() && 0 == i && (petmode = -1, k = 1e3), setTimeout(v, k) } for (o = 11; o-- > 0;)RunFireBall($, t, e, 320 + o % 2 * 8); $.insertBefore(a, $.firstChild), g(o = -50, 0), setTimeout(v, 2e3) }
const anim = document.getElementsByClassName('site-banner')[0];
anim.parentElement.parentElement.addEventListener('mouseover', () => { petescape = true; petfast = false; });
anim.parentElement.parentElement.addEventListener('touchstart', () => { petescape = true; petfast = false; });
let offset = document.documentElement.dataset.bsTheme == 'dark' ? 20 : 0;
RunPet(anim, anim.offsetWidth, anim.parentElement.parentElement.offsetHeight + offset, 320, anim.parentElement.parentElement.offsetHeight - 16 + offset);