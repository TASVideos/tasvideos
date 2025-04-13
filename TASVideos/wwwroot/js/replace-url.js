const canonicalUrl = document.querySelector("link[rel='canonical']")?.href;
if (canonicalUrl) {
	history.replaceState(null, '', canonicalUrl + location.hash);
}
