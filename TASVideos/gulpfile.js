/// <binding AfterBuild='default, clean, copy' />
const gulp = require("gulp");
const rimraf = require("rimraf");

// pre-minified stuff to copy over
const preminified = [
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

gulp.task("clean", (cb) => {
	rimraf("wwwroot/lib", cb);
});

gulp.task("copy", ["clean"], () => {
	return gulp.src(preminified.map(s => "node_modules/" + s), { base: "node_modules" })
		.pipe(gulp.dest("wwwroot/lib"));
});

gulp.task("default", ["copy"]);
