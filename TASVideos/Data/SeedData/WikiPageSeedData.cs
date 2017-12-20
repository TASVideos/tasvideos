using TASVideos.Data.Entity;

namespace TASVideos.Data.SeedData
{
	public class WikiPageSeedData
	{
		private const string InitialCreate = "Initial Create";
		public static readonly WikiPage[] SeedPages =
		{
			new WikiPage
			{
				PageName = "HomePages",
				RevisionMessage = InitialCreate,
				Markup =
@"[TODO]: make this page nicer
This is a list of homepages for various users.

All users, by default, have the ability to create and edit their homepage.  Log in, and navigate to HomePages/[[your username]] and click the edit button.
----
[module:listsubpages]
"
			},
			new WikiPage
			{
				PageName = "Wiki",
				RevisionMessage = InitialCreate,
				Markup =
@"In general, a '''[http://en.wikipedia.org/wiki/Wiki|wiki]''' is a site designed
to be freely editable, arguably most well known for sites such as
[http://wikipedia.org/|Wikipedia].  In particular, much of this site's content
is maintained in a wiki which you are looking at at this very moment.

We encourage anyone with interest to become an [EditorGuidelines|editor] for the
site; once you have an account just ask [Users|an admin assistant or site admin]
for editor priviliges.

----
See also:
* [Users], listing current editors (and staff members) for the site
* [Editor Guidelines]
* [Text Formatting Rules] for information about wiki markup"
			},
			new WikiPage
			{
				PageName = "WikiOrphans",
				RevisionMessage = InitialCreate,
				Markup =
@"This page is intended to be useful
for [Wiki] librarians, aka. [Users|Editors].

!!! Wiki orphans

Wiki orphans are pages which exist but have no references.
This page lists the pages that currently are in that unhappy state, excluding [Homepages].

[module:WikiOrphans|maxrefs=0|pagetypes=P]

!!! Broken links
These are non-existing pages that are linked to:

[module:BrokenLinks]"
			},
			new WikiPage
			{
				PageName = "TODO",
				RevisionMessage = InitialCreate,
				Markup =
@"This page documents various projects that have been suggested for the site.  If you are looking to [Helping|help], this is a good place to look for ideas.

%%TOC%%

!!! Wiki
If you want to help with wiki pages, you will need to be an [EditorGuidelines|editor].  If you have a project in mind, contact [Users|an admin assistant or site admin] with details and you will probably be granted editor access.

!! Various page TODOs
* Many pages which [=Wiki/Referrers?path=TODO|refer] to this TODO page have changes which the editors feel could be made. Be aware that some pages (such as [ArticleIndex]) link to TODO but not for this reason.

!! Adopt the orphans
* Pages with no links to them, as well as non-existent pages linked from somewhere are found in [Wiki Orphans]; links to these pages should be added elsewhere.  This is an ongoing problem.  It is HIGHLY encouraged that all staff & editors participate in keeping these Orphans fixed.
** Also consult [user:sgrunt]'s [Sgrunt/PseudoOrphans|pseudo-orphans] list for pages with few links pointing to them.

!! [Game Resources]
If you know a game well, write a page about it. It is as simple as that.

!! Pages needed
*Write missing pages ([LawsOfTAS]) detailing the implementation of rerecording, from a coding perspective.  This needs to include movie format specifications, including pros and cons of various methods already tried.  Also should include the ""bullet proof"" rerecording logic.  All aspects that are required for TASVideos acceptance should be explained clearly as well as detailing how to achieve these results.  

!! Pages in bad need of improving
* [Downloads]
** All tools linked should be uploaded to the TASTools repository instead (and linked to it).

!! ""Game Ideas"" list
We need a good way of generating such a list.  So far all ideas have been impractical or neglected.

!!! Movies
Some changes here need [Users|vested editor] access or above.
!! Old movies
* Finish the ""History Revival"" project.  This includes about 80 movies at around 650M.  These movies should have proper encodes with archive or streaming URLs, screenshots, and descriptions.

!! Movie filenames
In [=forum/t/4678|this forum thread] ... plan and implement the changing of all movie files to something that makes more sense.
*Vested editors and above need access to be able to modify movie file names.

!!! Site
Most changes here will probably need to be done by [Users|admin-level staff].  Feel free to add suggestions.

!! Technical Changes
! Edit conflict warning
The site now uses diff3 to merge edits from two or more editors
who cross-edit the same article.

However, when edits conflict, the site quietly
garbles the article without a warning. This should be fixed.

*Possibly issue a warning when starting to edit, when someone else is already editing the page
* It does not work together with the history editor(deletion of past versions changes the revision numbers)

!Security
* Bad document names are not verified

!Source code
* Reduce the number of SQL queries scattered around the code
* Add a special case in WikiEmbedJavascript and WikiLoadJavascript for tabbed context

!Database
* Change playerid so that a movie may have multiple playerids.Remove the multiplayer playerids.

* Transparent cache proxy for high CPU load situations(no dynamic code execution or POST methods).

!!Translation support
[Bisqwit]: Domainwise, Japan is the third biggest group of visitors
of this site.I like that.Unfortunately, not many of the Japanese
understand English very well.It has been my long time aim to ease
the usability of this site for Japanese users.
Unfortunately, so far I haven't managed to invent/plan a way to
support alternate language versions of pages on this site.
But maybe something could be done about it.
"
			},
			new WikiPage
			{
				PageName = "TextFormattingRules",
				RevisionMessage = InitialCreate,
				Markup =
@"%%TOC%%

!! Introduction

All of the pages on this site use a special type of formatting,
which follows some features commonly found in [Wiki] systems.

The documents of this site (including submissions) ''are __not__ edited in HTML or bbCode''!

This page explains all of the features of this customized markup
language, which is developed with simplicity of editing in mind.

!! Paragraphs

* Words wrap and fill automatically. You do not need to bother with manually inserting line breaks..
* You can keep the lines short to make the diffs (page history) more easily readable.
* Separate paragraphs with a blank line.

Example: This is paragraph one.

This is paragraph two. There is a blank
line in between.

You can force a line break without making a new paragraph with %%''''%,
but it is not recommended. Example: %%%
This paragraph%%%
uses line breaks%%%
excessively.%%%

However, if your lines begin with a space, none of this happens.
See ''[=TextFormattingRules.html#PreformattedText|preformatted text]''.

!! Lists

* This is a bullet list.
* In order to create a bullet list, put * at the start of each line.
** You can get deeper hierachy by placing ** for second level, *** for third etc.

# This is a numbered list.
# In order to create a numbered list, put # at the start of each line.
## You can use ## and ### as in the previous example.
#* You can also mix # and *.

* New line stops the list, but you can use %''''%% (see above).
* ;Term:def makes a definition list:
Example:%%%
'''';Term to be defined: Your explanations appears here.%%%
'''';TAS: A tool-assisted speedrun.%%%
Turns into:
;Term to be defined: Your explanations appears here.
;TAS: A tool-assisted speedrun.

Hint: See the [=Wiki/ViewSource?path=TextFormattingRules|source code] of this page to see how the markup works.


!! Headings

* ‘! ’ at the start of a line makes a small heading.
* ‘!!’ at the start of a line makes a medium heading.
* ‘!!!’ at the start of a line makes a large heading.
* ‘!!!!’ at the start of a line makes a main heading. Because the system automatically creates a main heading for normal pages, this markup is only usable for [system pages].
* The command %''''%TOC%% creates a table of contents consisting of the headings.
* Usage of headings for structuring the document is recommended. 

Four or more minus signs at the start of a line make a horizontal ruler. Example:
--------

!! Emphasis

* Use doubled single-quotes ('____') for emphasis (''italics'').
* Use doubled underscore (_''''_) for strong emphasis (__bold__).
* Use doubled parentheses (____( and )____) for ((smaller text)). (Can be nested)
* Use doubled braces {____{ and }____} for {{teletype text}}.
* Use triple dashes (--''''-) for ---overstrike effect--- (when you wish to emphasize that something has been changed/removed from the page).
* Use double square quotes «''''« and »''''» for ««inline quotations (««Can be nested»»)»».
* Use right angle bracket (>) for indented quotes:
> This is an example quote.
* Emphasis can cross line boundaries, so be careful – a rogue “--''''-” for example can cause the entire page appear in strike-over.

!! Preformatted text

In order to use preformated text, such as indented code, use ''space'' as the first character of a line (monospace font).

Example: This is standard text
 This is preformatted text using a monospace font (space as first character).
 Here is an action scene:
    +_''''_-+-       “One image says more than a thousand words.”
  --   \_''''_|                                    _''''_''''_''''_''''_ _:
               _''''_''''_''''_''''_             _''''_''''_''''_    - -- |     | |_
      - - --''''- /  |_''''_\_''''_    --  _/ |_''''_\|_    - O  ~  |   \
    _  -   _''''_ `-o--''''--o-'_''''_    `-o--''''--''''-o-'  _''''_ `-oo--^--o-'

Notice the usage of '____'____'____' (four successive apostrophes) to break long strings of underscores to prevent them being interpreted as bold on/off.

Please use preformatted text for program source code and such only.
Preformatted text does not line wrap and can break page layout if used incorrectly.

!! References (Links)

[http://media.tasvideos.org/guidelines/entertain.gif|right]
* Internal links (to other wiki pages here)
** Link to wiki page names by enclosing them in brackets: [[RecentChanges]] or [[recent changes]].
** For internal links other than the wiki, see below.
** It is possible to give your link a different name by using square brackets and ‘|’ like this: [[FAQ|Questions and answers]] produces [FAQ|Questions and answers].
** To output brackets as-is, put them in doubles: [[[[ and ]]]]
* External links (automatically gets a [http://dummy| ] icon)
** URLs with {{http:}}, {{ftp:}} or {{m''''ailto:}} are automatically linked: http://www.google.com/ .
** URLs can be named in the same way as internal links: [[!http://www.google.com|Google it!]] produces [http://www.google.com|Google it!]
** You can suppress automatic hyperlink parsing by preceding the link with a ‘!’, e.g. !''''!http://not.linked.to/.
* Images
** URLs ending in {{.png}}, {{.gif}}, {{.jpg}} are inlined if put in square brackets (embedded images).%%% (((But when you embed images, please make sure that you have the permission to do so from the server where the image is being loaded from!) ))
** For internal links (relative), prefix the image's URL with a =.
** To left- or right-align an embedded picture, the link text can be “left” or “right” respectively. [[!http://media.tasvideos.org/guidelines/entertain.gif|right]] produces the image above.
** To include {{alt}} or {{title}} attributes in an image use {{alt=text}} or {{title=text}}.
** You can combine multiple image options by using '|' like this: [[=image.png|alt=Image of cows|title=Coolest image ever|left]] or this: [[http''''://images.google.com/image.png|alt=Image of cows|title=Coolest image ever|left]]
** You can create image links like this: [[!http://www.google.com|=googlelogo.png|alt=Google's Logo]] or this: [[!http://www.google.com|http''''://www.google.com/googlelogo.png|alt=Google's Logo]]

__Linking to submissions__

Construct the link from the movie ID and ""S"". For example, Rockman submission is #[1032S|1032], so the link is written as: [[1032S]].%%%
To add a ''custom'' description, write for example: [[1032S|rockman submission]]. The internal description can be seen when hovering over the link.

You can find the number from the URL of the submission.

__Linking to movies__

Construct the link from the movie ID and ""M"". For example, Rockman movie is movie[515M | 515], so the link is written as: [[515M]].%%%
To add a ''custom'' description, write for example: [[515M|rockman movie]]. The internal description can be seen when hovering over the link.

You can find the number at[List All Movies].

(Note: This was recently changed.Before it used to be = movies.cgi ? id = number)

__Linking to forum topics__

Construct the link from ""=forum/t/"" and the topic ID.For example, the[= forum / t / 629 | Gradius topic] is written as [[=forum/t/629|Gradius topic]].

__Linking to forum posts__

Construct the link from ""=forum/p/"" and post ID.For example, the[= forum / p / 227916#227916|forum rules] is written as [[=forum/p/227916#227916|forum rules]].

__Linking to forum profiles__

Construct the link from ""=forum/r/"" and user ID.For example, [user: Nach]'s [=forum/r/17|profile] is written as [[=forum/r/17|profile]].

Alternatively use ""=forum/r/"" and the username.For example, [user:Bisqwit]'s [=forum/r/Bisqwit|profile] is written as [[=forum/r/Bisqwit|profile]]. However, keep in mind that the link won't work after a user has their name changed.This is why using the user ID is preferred.

__Linking to other internal pages__

To link to any other page that does not have a {{.html} } in the URL,
take the URL and replace “!http://tasvideos.org/” with “=”%%%
e.g. [[= movies.cgi ? name = Mega + Man + X]], [[=Wiki/Referrers?path=TODO]], [[=forum/]]

* __Footnotes__: create links to footnotes with[[#1]] or any other number (i.e. square brackets + hash + number), and precede the footnote itself with [[1]] (i.e. square brackets + number). Example: [#1]

!!Tables

Tables are created with lines that begin with vertical bars. ( | ) %%%
Double vertical bars( || ) create headers.

Example:
 || header1 || header2 || header3 || header4 ||
 | field1a | field2a | | field4a |
 | field1b | field2b with[[|]] vertical bar|field3b with%%''''%a few words|field4b|
becomes:
||header1||header2||header3||header4||
|field1a|field2a| |field4a|
|field1b|field2b with[|] vertical bar|field3b with%%%a few words|field4b|
%%%
To output a literal vertical bar in a table, surround it in brackets:  [[|]].
To output empty cells, put a space between the cell edges.

To ensure that the table format and appearance is consistent (white lattice with border), every row should have one more vertical bar separator than the number of cells in a table row.

!! Mark-Up Language(HTML)

* Do not try to use HTML.HTML markup does not work.
* < and > and & are themselves, except at the beginning of a line.

!! Table of contents

* You can insert a table of contents to the page with the %''''%TOC%% macro. (See the top of this page for an example of its use.)
* The table of contents must come before any headers or else it will not work.

!! Tabs

To create tab entries, write as follows:

 %''''%TAB header_text1%%
 entry1
 %''''%TAB header_text2%%
 entry2
 ...
 %''''%TAB header_textn%%
 entryn
 %''''%TAB_END%%

For example:

 %''''%TAB Table of Pokémon%%
 Bulbasaur, Venusaur, Ivysaur, and hundreds of others.
 %''''%TAB Minimize tab%%
 %''''%TAB_END%%

produces:

%%TAB Table of Pokémon%%
Bulbasaur, Venusaur, Ivysaur, and hundreds of others.
%%TAB Minimize tab%%
%%TAB_END%%

__Important:__ There must be a newline after  %''''%TAB_END%% or else it will not work.

Note: Headings can not be embedded in tabs

! Directives:
__%''''%TAB_START%%__%%%
Start a tabset.Usually only needed if you want nested tabs.%%%
__%''''%TAB_HSTART%%__%%%
Start a tabset with tabs on left side instead of top.%%%
__%''''%TAB<name>%%__%%%
Create a new tab.Starts a tabset if there isn't one.%%%
__%''''%TAB_END%%__%%%
End the innermost tabset.%%%


* { {%% TAB_} }{{END%%}} is important.If it is missing, odd things happen.
* Tabs can be nested.
* There must be no headings in the tabs.
* Ending a tab ends any nested quotes and divs.

Note: In the current implementation of the site, tabs are implemented
with Javascript.It is not recommended to use them, because the tabs
are not usable with Javascript-challenged browsers.

!! Source code
__%''''%SRC_EMBED<hilighting> __%%%
Start block of code with specified hilighting type. Wiki markup is not processed inside this block.%%%
__%''''%END_EMBED__%%%
End a block of code.
%%SRC_EMBED lua
function factorial(n)
local x = 1
  for i = 2, n do

x = x * i

end
  return x
end
%%END_EMBED

!! Quotations
__%''''%QUOTE[< name >] __%%%
Start a quote block (If<name> is specified, that is shown to be quoted).%%%
__%''''%QUOTE_END__%%%
End a quote block.Automatically ends any nested tabs and divs.

%%QUOTE Example Person
Example Quote
%%QUOTE_END

!! Divs
__%''''%DIV<class> __%%%
Start a div block with given class <class>.%%%
__%''''%DIV_END__%%%
End a div block.Automatically ends any nested tabs and quotes.

%%DIV foo
Example Div
%%DIV_END

!! Comments and if macros

* To write something in the source without having it show up in the page, surround the comment with [[i''''f:0]]...[[endif]].
* Surrounding anything with[[i''''f:1]]...[[endif]] does nothing to the text.
* The wiki reserves some variables which evaluate to 1 if true and 0 if false:
** UserIsLoggedIn
** CanEditPages(1 if user is an ''[Users|editor]'')
** UserHasHomepage
** CanViewSubmissions(as for now, always 1)
** CanSubmitMovies(as for now, same as UserIsLoggedOn)
** CanJudgeMovies
** CanPublishMovies
** CanRateMovies(as for now, same as UserIsLoggedOn)

e.g. [[i''''f:!UserIsLoggedIn]]%%%(The ! reverses 1 and 0)

__Other macros__

*[[e''''xpr:UserGetWikiName]] returns the reader's wiki username (if logged on).
*[[u''''ser:user_name]] links to the homepage of user_name(if there is one).
*[[module:xxx]] inserts a module which performs some function.See the[TextFormattingRules / ListOfModules | list of modules] for more information.Some modules are restricted.
 
!! Character set

* This server uses unicode with UTF-8 encoding.This means that you can use all characters supported by unicode, including the Japanese characters. But please note that not everyone can read them.
 
----
[1]: This is an example of a footnote."
			},
			new WikiPage
			{
				PageName = "TextFormattingRules/ListOfModules",
				RevisionMessage = InitialCreate,
				Markup =
@"[module:ListParents]

!!! List of modules for use in wiki markup.

%%TOC%%

Note: Only a few of these are actually useable in practical markup.
Most of these modules are designed solely for the purpose of some
particular page.

!! accessmaps

 [[module:accessmaps]

Lists the access levels and actions each one has access to.

!! activetab

 [[module:activetab|tab=b1]

Indicates which tab of the main menu is assumed to be active, overriding the system selection. The first letter indicates the context of the tabs activated ('b' = main menu, 'a' = tiny menu), and the next letters indicate the tab number.

!! addresses

 [[module:addresses|addrset=id]

Embeds a table of addresses created in the [AddressesUp|addresses editor]. id is the address set to embed.

!! aviencodes

 [[module:aviencodes]

Shows listing of all .avi encodes still used (used in [AviEncodes]).

!! brokenlinks

 [[module:brokenlinks]

Lists broken links.

!! cachemanager, cachestatistics

 [[module:cachemanager]

Generates a cache flushing button. Access is restricted.

 [[module:cachestatistics]

Generates cache statistics.

Both used at [CacheControl], should not be placed somewhere else.

!! createtraplink

 [[module:createtraplink]

This module creates an invisible link that blocks the access of whoever
follows it. Regular viewers will never notice it, and well-behaving robots
won't follow it due to {{robots.txt}} restriction, but badly behaving robots
will follow it and get banned.

It is already included in the page layout. It should not be placed somewhere else.

!! DailyMotion

 [[module:dailymotion]

Generates an embedded [http://www.dailymotion.com/|DailyMotion] movie viewer.

Params:

|v=code|Code is the value after /video/ preceding the first _ in the DailyMotion URL.|Required
|w=width|Override the default width of 480.|Optional
|h=height|Override the default height of 425.|Optional
|align=direction|Align to the left or right by sending the direction to left or right.|Optional
|start=seconds|Amount of seconds into the video to start playing it from.|Optional
|loop=seconds|Amount of seconds into the video at which point it should loop back to the starting point. Currently buggy.|Optional
|hidelink|Specify this parameter to hide the link to DailyMotion's site on the bottom.|Optional

!! dbrelations

 [[module:dbrelations]

Lists database relations. Used at [SiteTechnology/Database].

!! diggbutton

Example:
 [[module:diggbutton|urn=programming/How_to_Write_and_use_SOAP_services_with_PHP_and_JavaScript_AJAX]

Creates a [http://www.digg.com/|Digg] button.
The digg submission for which it links to, must exist beforehand.
The button is automatically aligned on the right.

It should only be used on the pages corresponding to the digg submission.

!! displaygamename

Displays the name of specified game or games (according to display name stored in gamenames database table)

Params:
|gid=gameid|Comma-separated list of game IDs.|Mandatory except on special pages

The following pages are special (optional gid):
* Submission pages. Game defaults to whatever game is set for the submission.
* Movie pages. Game defaults to whatever game is set for the publication.
* Linked gameresources pages (and subpages thereof): Game(s) having that as resources page.
* Any subpages included by pages above.

Examples:
* [[module:displaygamename|gid=634] shows [module:displaygamename|gid=634].
* [[module:displaygamename|gid=634,709] shows [module:displaygamename|gid=634,709].

!! displaymovie

Example:

[[module:displaymovie|id=600]

Creates a movie listing, just like the Movies-*.html pages do. The parameters are the same as accepted by movies.cgi:
;id:Limits selection to individual movies matching the given ID (comma separate to select many)
;obs:If specified, allows displaying obsolete movies (only when id is not specified)
;obsonly:If specified, limits display to obsolete movies (only when id or obs is not specified)
;ratingsort:If specified, sorts movies by rating
;name:Limits selection to movies whose name match the given text (exact substring)
;playerid:Limits selection to movies played by the specified player entity (group/individual) (give ID)
;systemid:Limits selection to movies of games on the specified game console (give system ID, not system name)
;catid:Limits selection to movies where the given category (by ID) is specified (note: catval must also be specified)
;catval:Indicates the polarity of the category selection: 1 = positive, -1 = negative.
;maxage:Limits selection to movies not older than N seconds where N is the given parameter.
;rec:If specified (value does not matter), limits selection to movies having the ""recommended"" flag (i.e. star) set. If specified with ''notable'', combines as or.
;notable:If specified (value does not matter), limits selection to movies having the ""notable improvement"" flag (i.e. lightning) set. If specified with ''rec'', combines as or.
;verified:If specified (value does not matter), limits selection to movies having the ""verified"" flag (i.e. check) set.
;tier:Comma-separated list of tiers to filter for.
;flags:Comma-separated list of flags to filter for.
;game:Comma-separated list of game IDs to filter for.
;group:Comma-separated list of game group IDs to filter for.
;noshowtiers:Comma-separated list of tier names not to show icons for
;noshowflags:Comma-separated list of flag names not to show icons for.
!! editoractivity
 [[module:editoractivity]

Used at [MostActiveEditors].  Displays statistics on editors' edits to movies and wiki pages.

|limit=num|Total number of users to display (default 30).  Maximum 100.|Optional
|sort=field|Sort on total (default), wiki, or movie edits.|Optional

!! editpage

 [[module:editpage]

Creates an ""edit this page"" link. Access restricted.

!! editusers

 [[module:editusers]

Creates an user editor. Access restricted.
Is used at [Users/Edit], should not be placed somewhere else.

!! encodes10bit444

 [[module:encodes10bit444]

Lists 10 bit 444 encodes.

!! feedlog

 [[module:feedlog]

Displays a table for an RSS/Atom feed or SVN commit log from a public subversion repository.

RSS feeds are expected to contain items with a clean author, pubdate, link, and title elements.

Atom feeds are expected to contain entries with a clean author, modified or updated, link, and title elements.

Params:

|url=URL|URL to RSS/Atom feed or to subversion repository. For subversion repositories, can be trunk or another subdirectory.|Required
|type=TYPE|Type can be ''rss'', ''atom'', or ''svn''.|Required
|limit=number|Override the default amount (10) of entries to show.|Optional

!!firsteditiontas

[[module:firsteditiontas]

Display list of first publications for games.

Parameters:
|after=<timestamp>|Display only games with first publication at or after given unix timestamp.|
|before=<timestamp>|Display only games with first publication before given unix timestamp.|
|splitbyplatform|In addition of considering the game name, also consider the system.|
|vault|By default only movies outside the vault are displayed. Include this to display the vault tier as well.|

!! flowplayer

 [[module:flowplayer]

Displays a player for an MP4 or FLV video hosted anywhere on the Internet.

Params:

|v=URL|The full URL including ""{{http://}}"" at the beginning to a video file.|Required
|i=URL|Image to display while the movie is loading; use full URL.|Optional
|w=width|Override the default width of 640.|Optional
|h=height|Override the default height of 480.|Optional
|start=seconds|Time into the video to start playing at.|Optional
|align=direction|Align to the left or right by setting the direction to left or right.|Optional
|hidelink|Hide the link to download the video.|Optional

!! forumfeed
 [[module:forumfeed|f=ForumID]

Display the contents of a particular forum. Note, this module isn't polished yet.

!! forumposters

 [[module:forumposters]

Lists forum posters with their avatars.
Is used at [MostActivePosters], should not be placed somewhere else.

!! forumuser

 [[module:forumuser|name=username|id=userid]

Outputs ""this page appears to be homepage of user X"" blurb.
Is automatically used at homepage, should not be placed somewhere else.

!! forumuserfield

 [[module:forumuserfield|user=username|field=fieldtype]

Outputs the selected field from the given user's forum profile, verbatim.
Allowed fieldtypes are currently “signature” and “interests”.

!! frames

 [[module:frames]

Generates a human readable time from an amount of frames.

Params:

|amount=number|Amount of frames|Required
|fps=number|Override the default frames per second of 60.|Optional

If you specify {{ntsc}} or {{pal}} as the fps value instead of a number, the module will get the framerate for the current platform from the [PlatformFramerates|site's database]. Intended for movie descriptions.

!! frontpagemovielist

 [[module:frontpagemovielist|maxdays=n|maxrels=n]

Creates a movie listing for the front page. Currently unused.

!! frontpagesubmissionlist

 [[module:frontpagesubmissionlist|maxdays=n|maxrels=n]

Creates a submission listing for the front page.

!! gamefooter

 [[module:gamefooter]]

Creates game resource page footer. All game resource pages (except [GameResources] itself) should have this.

!! gameheader
 [[module:gameheader]]

Creates a game resource page header.  All game resource pages (except [GameResources] itself) should have this.

!! gamename
 [[module:gamename]]

Used by SystemGameResourcesHeader in order to generate the game name of a game resource page.  This module should not be used anywhere else.

!! gamenameseditor

Used by GamenamesEditor to place the Gamenames table editor there. Should not be placed anywhere else.

!! gamesubpages

Used by [GameResources] only.  Displays all Game Resources pages

!! getfirefox

 [[module:getfirefox]
 [[module:getfirefox|align=right]

Generates a ""Get firefox"" advertisement button.
Alignment can be specified.

!! googleflavor

 [[module:googleflavor]

Creates a site-flavored Google search form.

!! judgecounts

 [[module:judgecounts]

Lists number of decisions by judges.  Used on [Judge Counts].

Params:
| userid=<userid> | Display only given user ID[#1]|

!! jwplayer

 [[module:jwplayer]

Displays a player for an MP4 or FLV video hosted anywhere on the Internet.  Makes use of [[http://www.longtailvideo.com/ JWPlayer]].

Params:

|v=URL|The full URL including ""{{http://}}"" at the beginning to a video file.|Required
|i=URL|Image to display while the movie is loading; use full URL.|Optional
|w=width|Override the default width of 640.|Optional
|h=height|Override the default height of 480.|Optional
|start=seconds|Time into the video to start playing at.|Optional
|align=direction|Align to the left or right by setting the direction to left or right.|Optional
|hidelink|Hide the link to download the video.|Optional
|sub|A subtitle file to accompany the video.|Optional

!! linkgamenameresources

If game has linked game resources page, links there

Params:
|gid=gameid|Game ID.|Mandatory except on special pages
|linktext=base64|Base64 of link text to give.|Optional

May be placed without gid parameter on submission and publication pages.

!! listlanguages

If a page has translated versions in different languages, it will provide links to pages with those languages.

Optional parameter:
| istranslation=true | If set to true, it assumes it is a translated page and will look for and link to its English variant, and other translations of its kind.

!! listmovies, moviesearchcategory, moviesearchsystem

 [[module:listmovies]
 [[module:moviesearchcategory]
 [[module:moviesearchsystem] 

Creates the contents of the [MovieSearch] page. Should not be used somewhere else.

!! listparents

 [[module:listparents]

Generates backlinks to parent pages when hierarchical page structure is used.

!! listsubpages

 [[module:listsubpages]

Generates links to subpages when hierarchical page structure is used.

!! loginbar

 [[module:loginbar]

Generates the login functionality that is used in the topmost menu.

!! moviechangelog

 [[module:moviechangelog]

Lists history of movie updates and obsoletions.

Params:
| maxdays=n | Selects maximum days to list (optional)
| maxrels=n | Selects maximum releases to list (optional)
| float=1 | If specified, makes it float on the right like on the front page. Also embeds an advertisement. (optional)
| seed=id | Specifies the list of movieids for whose history to list (optional)
| heading=text | Specifies a heading (optional)
| footer=text | Specifies a footer (optional)
| flink=WikiPageName | Specifies a page to link the footer text to. Requires footer to be set. (optional)

!! movielistbyname

 [[module:movielistbyname]

Lists movies by name.

!! moviemaintlog

 [[module:moviemaintlog|type=letter]

Used at [Flag Maintenance Log], [Movie Maintenance Log] and [Tier Maintenance Log]. Should not be placed somewhere else.

Params:

(C)ategory, (F)ile, F(L)ag, (H)eader, (R)ecommended, (T)ext, T(I)er.

!! movienamesearch

 [[module:movienamesearch]

Outputs a form for searching movies by name. Obsoleted by the [[searchbar]] module.

!! moviesbyplayer

[[module:moviesbyplayer]

Display list of movies grouped by player

Parameters:
|after=<timestamp>|Display only movies published at or after given unix timestamp.|
|before=<timestamp>|Display only movies published before given unix timestamp.|
|newbies=show|Mark newbies (first publication in given time interval).|
|newbies=only|Only show newbies (first publication in given time interval).|

!! movieslist

  [[module:movieslist]

Body of [MoviesList] pages.
If invoked without parameters, outputs list for all systems.

Params:
| platform=<id> | Only list movies for given platform (instead of all platforms) |


!! moviestatistics

 [[module:moviestatistics]

Lists statistics of movies.%%%
If invoked without parameters, lists generic statistics.

Params:
| comp=[[-]]fieldname | specifies field to compare using
| need=fieldname | specifies which field must have nonzero value
| fields=fieldname=fielddesc{,...} | lists fields to list
| top=n | specifies the number of results to output (default 10)
| minAge=n | specifies the minimum age of the movie in days to be listed

List of available fields is subject to change.
Currently it is:%%%
id, length, filererecords, playerid, recommended, alength, asize, aname,
desclen, num_lines, udesclen, formattedLength, formattedALength, compressionRatio, alengthPerLength, alengthMinusLength, formattedALengthMinusLength, btDownloaded, btDownloadedMegs, daysPublished, downloadsPerDay.

!! nicovideo

 [[module:nicovideo]

Generates an embedded [http://www.nicovideo.jp/|Nico Nico Douga (nicovideo)] movie viewer.

Params:

|v=code|Code is the component after the final / of the URL.|Required
|w=width|Override the default width of 728.|Optional
|h=height|Override the default height of 410.|Optional
|align=direction|Align to the left or right by sending the direction to left or right.|Optional
|hidelink|Specify this parameter to hide the link to Nico Nico Douga's site on the bottom.|Optional

!! nogamename

Used by NoGameName to place the listing of submissions having no assigned game name there. Should not be placed anywhere else.

!! nvacalc

 [[module:nvacalc]

NesVideoAgent's calculator, used at [NesVideoAgent/Calc].

!! pubcounts
 [[module:pubcounts]

Counts number of publications by publisher, as seen on [Pub Counts].

Params:
| userid=<userid> | Display only given user ID[#1]|
| publist | In conjunction with userid, display all publications by that user ID|

!! publicationsfromnewesttooldest

[[module:publicationsfromnewesttooldest]

Display list of publications from newest to oldest.

!! rssbutton

 [[module:rssbutton]

Generates a rss button.

!! searchbar

 [[module:searchbar]

Generates a tiny [Search] form.

!! settableattributes

 [[module:settableattributes|pattern=regexp|style=cssstyle]

After a call to this module, subsequent table cells matching
the given regexp will be given the specified CSS style.
It has effect during the particular page's rendering,
and does not affect other pages.

!! similarmovies

 [[module:similarmovies|seed=movieidlist]

Lists movies similar to the given list.

!! submissionsbygamename

Displays list of known publications and submissions for given game names(s).

Params:
|gid=gameid|Comma-separated list of game IDs.|Mandatory except on special pages

The following pages are special (optional gid):
* Submission pages. Game defaults to whatever game is set for the submission.
* Movie pages. Game defaults to whatever game is set for the publication.
* Linked gameresources pages (and subpages thereof): Game(s) having that as resources page.
* Any subpages included by pages above.

!! submitmovie

 [[module:submitmovie]

Generates a movie submission form.

This is used at [SubmitMovie], and should not be used somewhere else.

!! topicfeed

  [[module:topicfeed|t=TopicID|l=Limit|heading=text]

This module can be used to display the posts of a particular forum topic in reverse order. The paramter t= should be set to the topic ID for the thread. The optional l= parameter can be used to change the default limit of how many posts to display. The optional heading= parameter can be used to add a heading title. The optional hidecontent parameter will make content hidden until clicked on.
This is used at the [News] page.

!! torrentdisplay

 [[module:torrentdisplay]

Lists torrents in need of seeding.
This module is used at [Helping].

!! tracker

 [[module:tracker]

Outputs tracker statistics. This module should not be used on
user pages, because of the heavy processing it requires every
time invoked.

!! unmirroredmovies

 [[module:unmirroredmovies]

Lists movies without mirrors, streaming links, or some combination thereof.  By default it lists all unmirrored movies.  Used on [Unstreamed Movies].

Params:
|streamed|Lists unmirrored movies with streaming links.|
|unstreamed|Lists unmirrored movies without streaming links.|
|allunstreamed|Lists all movies without streaming links (or where the only streaming link is from Viddler).|
|current|Lists un-obsoleted movies only|
|obs|List obsoleted movies only|

!! uploaders
 [[module:uploaders|id=ID]

Lists uploads by user.

Params:
|id=ID|User ID (see [=Uploaders-List.html|Uploaders List] for lists). Required|
|current|Only display current movies (the default is to display both current and obsolete)|
|obs|Only display obsolete movies (the default is to display both current and obsolete)|

!! usermovies
  [[module:usermovies|user=IDs|game=IDs|limit=maxentries]

List latest user movies

Params:
|user=IDs|List only files from those users if specified (comma separated)|
|game=IDs|List only files for those games (comma separated)|
|limit=maxentries|Override the default limit of 25 files|
|links=1|Display links for more and personal uploads|


!! usersearch
 [[module:usersearch]

Generates an HTML5+JS user search form which can be tied to pages which take a username as a parameter.

Params:

|where=loc|Location to be searching for users. Current options are ''site'' and ''forum''.|Required
|baseurl=URL|URL to the page the form submits to, included needed query parameters.|Required
|name=param|The name of the parameter in the query that specifies the user.|Required
|focus|Set the browser focus to the search field on page load. Note: only one item per page can have autofocus.|Optional

Example: [[module:usersearch|where=site|baseurl=/Users/Edit.html?mode=uedit|name=id]

This will create a form which upon submit will direct to ''/Users/Edit.html?mode=uedit&id=__<selected username>__''.

!! viddler
 [[module:viddler]

Generates an embedded [http://www.viddler.com/|Viddler] movie viewer.

Params:

|v=URL|URL of the movie on Viddler's site.|Required
|w=width|Override the default width of 545.|Optional
|h=height|Override the default height of 478.|Optional
|align=direction|Align to the left or right by setting the direction to left or right.|Optional

!! wikilogin

 [[module:wikilogin]

Generates the login form.
Used at [Login], should not be used somewhere else.

!! wikiorphans

 [[module:wikiorphans]

Lists [wiki orphans].

Params:
| pagetypes=list | selects page types to list (commaseparated list of P,M,S)
| maxrefs=n | maximum references to consider the page for listing

Used at [WikiOrphans].

!! wikireferences

 [[module:wikireferences]

Lists page referrers.

!! wikisearch

 [[module:wikisearch]

Generates the contents of the [Search] page. Should not be used somewhere else.

!! YouTube

 [[module:youtube]

Generates an embedded [http://www.youtube.com/|YouTube] movie viewer.

Params:

|v=code|Code is the value of the v= parameter in the YouTube URL.|Required
|w=width|Override the default width of 425.|Optional
|h=height|Override the default height of 325.|Optional
|align=direction|Align to the left or right by sending the direction to left or right.|Optional
|start=seconds|Amount of seconds into the video to start playing it from.|Optional
|loop=seconds|Amount of seconds into the video at which point it should loop back to the starting point.|Optional
|hidelink|Specify this parameter to hide the link to YouTube's site on the bottom.|Optional
|flashblock|Specify this parameter to not load any flash directly, and require JavaScript button processing to load & play.|Optional

!! Password generator / decoder modules

cv2upassword, faxpassword, glpassword, metropassword, mm2password, mm3password, mm4password, mm5password, mm6password, olympuspassword, solarjetpassword.

Password generator / decoder modules used at [PasswordGenerators].

!! Monetizing modules

adbriteads, admultiplex, donate, donatebutton, googleads, yahooads.

Placing these modules requires [user:adelikat]'s or [user:Nach]'s explicit permission.
That is, do not use them.
----
[1]: Search for your username [=Users/Edit.html?mode=uedit|here] to know your user ID."
			},
			new WikiPage
			{
				PageName = "System/WikiEditHelp",
				RevisionMessage = InitialCreate,
				Markup =
@"Refer to the [TextFormattingRules] for markup and formatting help.

Make sure all edits conform to the [EditorGuidelines]"
			}
		};
	}
}
