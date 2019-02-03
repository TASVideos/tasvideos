const fs = require("fs-extra");
const path = require("path");

function walkSync(dir) {
	const fileList = [];
	function walkSyncR(at, prefix) {
		const files = fs.readdirSync(at);
		for (const f of files) {
			const stats = fs.statSync(path.join(at, f));
			if (stats.isDirectory()) {
				walkSyncR(path.join(at, f), path.join(prefix, f));
			} else if (stats.isFile()) {
				fileList.push(path.join(prefix, f));
			}
		}
	}
	walkSyncR(dir, "");
	return fileList;
}

const srcDir = path.join(__dirname, "node_modules");
const dstDir = path.join(__dirname, "wwwroot", "lib");

const srcNames = [
	"jquery/dist/jquery.min.js",
	"jquery-validation/dist/jquery.validate.min.js",
	"jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js",
	"font-awesome/css/font-awesome.min.css",
	"font-awesome/fonts/fontawesome-webfont.eot",
	"font-awesome/fonts/fontawesome-webfont.svg",
	"font-awesome/fonts/fontawesome-webfont.ttf",
	"font-awesome/fonts/fontawesome-webfont.woff",
	"font-awesome/fonts/fontawesome-webfont.woff2",
	"bootstrap/dist/css/bootstrap.min.css",
	"bootstrap/dist/js/bootstrap.bundle.min.js"
];

fs.ensureDirSync(dstDir);
for (const name of walkSync(dstDir)) {
	if (!srcNames.includes(name.replace(/\\/g,"/"))) {
		console.log("Removing " + name);
		fs.unlinkSync(path.join(dstDir, name));
	}
}
for (const name of srcNames) {
	const srcp = path.join(srcDir, name);
	const dstp = path.join(dstDir, name);
	const srcStat = fs.statSync(srcp);
	const dstStat = fs.existsSync(dstp) && fs.statSync(dstp);

	if (!dstStat) {
		console.log("Creating " + name);
		fs.ensureFileSync(dstp);
		fs.copyFileSync(srcp, dstp, { preserveTimestamps: true });
	} else if (srcStat.mtimeMs > dstStat.mtimeMs) {
		console.log("Updating " + name);
		fs.copyFileSync(srcp, dstp, { preserveTimestamps: true });
	}
}
