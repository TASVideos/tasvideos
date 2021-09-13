# Frontend UI and Development Design Goals
The TASVideos site design revolves around Bootstrap 5.0 classes with minimal adjustments to preserve some of the original TASVideos site's theme colors and iconography. This document will outline the processes intended to be used for the custom styles and the guidelines for implementing bootstrap in a consistent manner.

## Code Structure
`TASVideos/wwwroot/css/` contains all custom styling intended for cross-site use. CSS customizations intended to be used only in a specific component should use inline styles, but that should be a last resort in case Bootstrap classes are missing something needed by a single component.

Stylesheets in the root of `TASVideos/wwwroot/css/` are 3rd party libraries and 1st party bundles of styles from the `partials` folder.

### Our Bundles
- `site.scss`, the main bundle for all cross-site styles.
- `darkmode-initial.scss`, the darkmode stylesheet loaded initially by the site if the user has not manually toggled dark mode.
- `darkmode.scss`, the stylesheet used when forcing darkmode with the toggle to override system settings.

### Third Party Bundles
- `prism.css`, a library used for code syntax highlighting
- `diffview.css`, a library used for diff highlighting

### Partials
The `partials` folder contains styles intended to be imported by the previously mentioned bundles. The partials are prefixed with an underscore as this is commonly used to help direct SASS compilers not to generate bundles, which will be helpful if we switch SASS compilers to support sourcemaps.

The `colors` partial defines variables for any colors to be used by other partials, enabling any custom colors used by the site to be adjusted from a single source of truth. The `variables` partial defines any other variables like pixel or rem amounts (preferred) which may be repeated throughout partials.

To avoid duplicate imports, the other partials do not use any imports and all importing is done when defining bundles. Syntax highlighting of the variable references is picked up via the Intellisense reference paths to `variables` and `colors` at the top of the other partials.

The main other partials to look at are `bootstrap-overrides` and `customizations`. These are split up to make it easier to follow which styles are set specifically to override Bootstrap 5's native behavior, and which styles are needed to implement site behavior unrelated to Bootstrap.

Two caveats for working with the partials:

- To use a SASS variable (dollar sign prefixed) as the value of a native CSS variable (prefixed with two dashes), the SASS variable must be wrapped with `#{}`
- Some native CSS functions like `RGBA()` need to be capitalized to avoid being picked up as the SASS builtin function `rgb()`, this way they can be used with SASS variables as expected.

## Code Standards

Variables that may change at runtime should be set in native CSS Custom Properties (variables). This reduces a ton of code duplication that would otherwise have been needed in past CSS. For example, dark mode may change the color of the site during runtime. To implement dark mode in Bootstrap 4, CSS would need to be rewritten for every component to override colors, essentially defining an entire 2nd copy of Bootstrap and requiring maintenance of two separate stylesheets. Because Bootstrap 5 makes heavy use of Custom Properties in its components, we can simply override those variables to change the colors used by most of Bootstrap's components.

Likewise, variables that are static at runtime should be set using only SASS variables.

Element `id` values and custom CSS `class` values should be kebab-cased (that is, lowercase and separated by dashes), matching the convention modeled by Bootstrap.

Custom css class names should follow Bootstrap's use of [Block-Element-Modifier](http://getbem.com/naming/) syntax. Block refers to a component, element to a specific element in that component, and modifier to the change itself. By naming classes this way, you can avoid needing to nest selectors (selector nesting has heavy performance penalties).

For example, to style a published movie's header using custom styles surrounding the site's secondary color
- Good: set class `publication-header-secondary` and define the needed styles with the selector `.publication-header-secondary` in `partials/_customizations.scss`
- Bad: set class to `publication secondary` and style with the selector `h1.publication.secondary` in partials.
- Even worse: defining custom styles like this if they can be set using preexisting bootstrap classes!

Bootstrap classes should be used wherever possible in favor of writing custom styles

## Style Guide

### Philosophy
Per MemoryTAS: I want it to be familiar, that we're not going in a radically different direction, but I want it to be clear that we are on the verge of a new era for TASVideos. I want it clear to everyone who looks at the new site that TASVideos is not only willing, but able to change as needed.

### Accessibility
Every page should have exactly one h1 element. This helps indicate the core content of a page to both search engines and to users with accessibility needs like screen readers.

The site should aim for WCAG compliance with respect to text contrast. This can be achieved mainly by overriding Bootstrap as little as possible. Custom colors should be run through Adobe's public [accessible palette picker](https://color.adobe.com/create/color-accessibility) to ensure differentiation between backgrounds and text.


### Content consistency

All site content below the nav bar should have a parent div with the Bootstrap `container` class.

Content should generally be left aligned, except for content in banners such as the site welcome and forum welcome.

The site's color theming revolves around deep purple and light blue accent colors, with black text against light grey backgrounds on the light theme, and light white text against dark grey backgrounds on the dark theme with no or little change to the accent colors. Exact values to be determined per adjustments for accessibility and reduction of potential duplication in the `colors` partial.

The purple accent color is intended for the navbar background and for button controls that don't change pages. The blue accent color is intended for links and button controls that change pages. These are also useful to separate functional groups of buttons.

Headings currently are inconsistent, a possible direction could be to use lighter info blue backgrounds from Bootstrap with black text on the light theme, and the inverted info (a deep navy blue) with white text on the dark theme.

Per [issue #399](https://github.com/adelikat/tasvideos/issues/399), developers seem to be in agreement that usage of overflow scrolls should be avoided where possible. For Y scrolls many of theme are simply not needed anymore with the current site design. Some X scrolls are currently used to hotfix usage of `table` elements which were overflowing mobile widths, the table elements are planned to be replaced by grid or flexbox.

Clickable buttons that act as links should be visibly differentiable from informational elements like titles. This may be possible simply by removing our override of bootstrap's text-decoration: underline on anchor elements as we don't use underline elsewhere on the site, but in any event link buttons should have some visual separation besides just usage of the accent theme colors. These buttons should also maintain the default border radius set by bootstrap for a consistent rounding look across the site.

Some custom icons are used for an 8 bit retro theme, for example the movie tier labels including the star, moon, and verification icons. these icons have an additional 2x and 4x exact prescale, enabling browsers to pick the appropriate image size to get crisp 8 bit edges with the use of responsive image sourcesets. All other icons needed should be pulled from the third party Font Awesome library which is included across the site (version 4.7.0).

Seperate components should be differentiated with the use of outlines in the primary accent color (this needs extensive work for consistency, main compliant examples are forum posts and movie lists).
