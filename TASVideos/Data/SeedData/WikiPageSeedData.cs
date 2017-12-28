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

Hint: See the [=wiki.exe?page=TextFormattingRules&mode=source|source code] of this page to see how the markup works.


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

Construct the link from the movie ID and ""M"". For example, Rockman movie is movie [515M|515], so the link is written as: [[515M]].%%%
To add a ''custom'' description, write for example: [[515M|rockman movie]]. The internal description can be seen when hovering over the link.

You can find the number at [List All Movies].

(Note: This was recently changed. Before it used to be =movies.cgi?id=number)

__Linking to forum topics__

Construct the link from ""=forum/t/"" and the topic ID. For example, the [=forum/t/629|Gradius topic] is written as [[=forum/t/629|Gradius topic]].

__Linking to forum posts__

Construct the link from ""=forum/p/"" and post ID. For example, the [=forum/p/227916#227916|forum rules] is written as [[=forum/p/227916#227916|forum rules]].

__Linking to forum profiles__

Construct the link from ""=forum/r/"" and user ID. For example, [user:Nach]'s [=forum/r/17|profile] is written as [[=forum/r/17|profile]].

Alternatively use ""=forum/r/"" and the username. For example, [user:Bisqwit]'s [=forum/r/Bisqwit|profile] is written as [[=forum/r/Bisqwit|profile]]. However, keep in mind that the link won't work after a user has their name changed. This is why using the user ID is preferred.

__Linking to other internal pages__

To link to any other page that does not have a {{.html}} in the URL,
take the URL and replace “!http://tasvideos.org/” with “=”%%%
e.g. [[=movies.cgi?name=Mega+Man+X]], [[=ref.exe?page=TODO]], [[=forum/]]

* __Footnotes__: create links to footnotes with [[#1]] or any other number (i.e. square brackets + hash + number), and precede the footnote itself with [[1]] (i.e. square brackets + number). Example: [#1]

!! Tables

Tables are created with lines that begin with vertical bars. ( | ) %%%
Double vertical bars ( || ) create headers.

Example:
 ||header1||header2||header3||header4||
 |field1a|field2a| |field4a|
 |field1b|field2b with [[|]] vertical bar|field3b with%%''''%a few words|field4b|
becomes:
||header1||header2||header3||header4||
|field1a|field2a| |field4a|
|field1b|field2b with [|] vertical bar|field3b with%%%a few words|field4b|
%%%
To output a literal vertical bar in a table, surround it in brackets:  [[|]].
To output empty cells, put a space between the cell edges.

To ensure that the table format and appearance is consistent (white lattice with border), every row should have one more vertical bar separator than the number of cells in a table row.

!! Mark-Up Language (HTML)

* Do not try to use HTML. HTML markup does not work.
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
Start a tabset. Usually only needed if you want nested tabs.%%%
__%''''%TAB_HSTART%%__%%%
Start a tabset with tabs on left side instead of top.%%%
__%''''%TAB <name>%%__%%%
Create a new tab. Starts a tabset if there isn't one.%%%
__%''''%TAB_END%%__%%%
End the innermost tabset.%%%


* {{%%TAB_}}{{END%%}} is important. If it is missing, odd things happen.
* Tabs can be nested.
* There must be no headings in the tabs.
* Ending a tab ends any nested quotes and divs.

Note: In the current implementation of the site, tabs are implemented
with Javascript. It is not recommended to use them, because the tabs
are not usable with Javascript-challenged browsers.

!! Source code
__%''''%SRC_EMBED <hilighting>__%%%
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
__%''''%QUOTE [<name>]__%%%
Start a quote block (If <name> is specified, that is shown to be quoted).%%%
__%''''%QUOTE_END__%%%
End a quote block. Automatically ends any nested tabs and divs.

%%QUOTE Example Person
Example Quote
%%QUOTE_END

!! Divs 
__%''''%DIV <class>__%%%
Start a div block with given class <class>.%%%
__%''''%DIV_END__%%%
End a div block. Automatically ends any nested tabs and quotes.

%%DIV foo
Example Div
%%DIV_END

!! Comments and if macros

* To write something in the source without having it show up in the page, surround the comment with [[i''''f:0]]...[[endif]].
* Surrounding anything with [[i''''f:1]]...[[endif]] does nothing to the text.
* The wiki reserves some variables which evaluate to 1 if true and 0 if false:
** UserIsLoggedIn
** CanEditPages (1 if user is an ''[Users|editor]'')
** UserHasHomepage
** CanViewSubmissions (as for now, always 1)
** CanSubmitMovies (as for now, same as UserIsLoggedOn)
** CanJudgeMovies
** CanPublishMovies
** CanRateMovies (as for now, same as UserIsLoggedOn)

e.g. [[i''''f:!UserIsLoggedIn]]%%%(The ! reverses 1 and 0)

__Other macros__

* [[e''''xpr:UserGetWikiName]] returns the reader's wiki username (if logged on).
* [[u''''ser:user_name]] links to the homepage of user_name (if there is one).
* [[module:xxx]] inserts a module which performs some function. See the [TextFormattingRules/ListOfModules|list of modules] for more information. Some modules are restricted.

!! Character set

* This server uses unicode with UTF-8 encoding. This means that you can use all characters supported by unicode, including the Japanese characters. But please note that not everyone can read them.

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
@"Refer to the [TextFormattingRules] for markup and formatting help.%%%
Make sure all edits conform to the [EditorGuidelines]"
			},
			new WikiPage
			{
				PageName = "EditorGuidelines",
				RevisionMessage = InitialCreate,
				Markup =
@"!!! Guidelines for wiki editors

%%TOC%%

! Requirements

* Ability to write text that conforms to these guidelines and follows the [Text Formatting Rules].
* Have the desire to actively maintain site articles, movie descriptions, or fixing grammar/spelling/punctuation/formatting errors on the site.
* Want to become an editor?  Simply ask a [Users|site admin or admin assistant] (either by forum PM or by IRC).

When editing pages just keep the following rules in mind.

! General

* Be considerate, especially when correcting a mistake another editor has made.
* Leave some time after another contributor's edit before editing a page yourself; they may have minor corrections to make.
** Also, try not to edit pages in rapid succession yourself.  Minor corrections are acceptable, but please try to accomplish what you are trying to do with as few edits as possible.
* No marketing speech. Write from a neutral point of view.
* You will not be able to edit system pages; this includes pages with ""System"" in the title and a handful of other significant pages.

! Minor edits

__Important:__ Multiple edits made by the same editor within half an hour of each other ''no longer'' count as just one edit in [Recent Changes]. The half hour is now reduced to one minute.

* As of now, all non-minor edits are reported immediately to IRC. If you are testing functionality or are just an obsessive editor, either use the preview button, or check minor edit so your edits do not disturb IRC. After you are sure that you are done, you can make a dummy edit and save without checking the minor edit box, if you make a significant change. This includes the [Sand Box].
* Please don't do mass changes (a small change that is done to 10 pages at the same time), unless you have discussed their need with other editors. Each edit creates a new copy of the page in database, and the site administration prefers quality over quantity.

! Understandable writing

* Write in a way that is easy to understand. Avoid slang and internet shorthand. For some of the site's viewers, English is not their first language.
* Structure your text carefully, so that the reader does not have to spend unnecessary effort in understanding what is written:
** Exercise good spelling, grammar, and structure.
** Use active words. Avoid using passive words like “get” and “have”. They are easily misunderstood and also don't machine-translate well.
** Avoid acronyms unless they are well-established in their context. It may help to describe what an acronym stands for the first time it is used on a page.

! Editing and commenting

* The main rule is to contribute anonymously and without a personal reference.
* If you represent yourself (“I think...”), you should prefix your comment with a link to your homepage (“[Bisqwit]: I think...”) or suffix it (“I think... --[Bisqwit]”).
* The purpose of the edit summary is to describe to others what you changed. Do not use the edit summary as a discussion vehicle. If you must discuss something, do it in the page, on the discussion forum, or in IRC.
** Keep in mind that your edit summary, if it is a non-minor edit, will normally be posted to [IRCChannel|IRC] so as to keep others informed of your work.

! Wiki markup guidelines
* __Learn the [text formatting rules].__ If you are familiar with text formatting, it becomes much easier to structure your writing.
* Do not overuse emphasis.
* If referring to tasvideos users by username, use the __[[user:foo]__ syntax (instead of __foo__ or __[[foo]__) so it will properly link to the user's homepage if or when the user has one. 

! Creating new pages

Step by step guide:
# Think of the purpose of this site. Does your planned page serve the audience of the site?
# Think of a name for your page. The name should contain the essence of the page’s topic. For example, a page of tips and tricks for Rygar would be “[GameResources/NES/Rygar|Rygar Tricks]”.
#* Don't use characters beyond A-Z, a-z, 0-9 and '/'.
#* The name should be as short as possible (for easy remembering and easy linking). Do not use acronyms (see above).
#* If there is danger of confusion (there are multiple different games by the same name, but you are writing only about one of them, for example), add more words to the name.
# Edit an existing page and add a link to that new page. The [Text formatting rules] explains how to create links. This step is to avoid it becoming a [Wiki orphans|Wiki orphan].
# Then click the newly created link, which will take you to the page you wanted to create.
# Click the Edit link on that page.

----
! Movie editing

Most of the following features are available only to vested editors.

Editors will see an small edit button on the bottom right hand side of a movie module.  This allows them to access the movie edit page.

{{Description}} (also available to normal editors)

Edits the movie description text.

{{Classes}}

Ability to edit the movie categories (such as ""Heavy Glitch Abuse"").  Clicking ""edit classes"" brings up a list of available classes.

When editing movie classes, follow the [Movie Class Guidelines].

{{Files}} (also available to encoders)

Ability to add/remove screenshots, torrent files, mirror and streaming urls.  Accessed from the Edit/add files button.

* If adding a screenshot, remember to click the delete checkbox for the existing one.
* Never remove the only torrent file for a movie.
If adding a torrent as a replacement, remember to click the delete checkbox for the existing one.
* All publications must have at least one valid streaming URL and/or an archive.org mirror URL.

{{Headers}}
Ability to edit the Player name, Platform, Game name, Game version, Branch name, ROM filename and the ""Hacked movie"" status.  Accessed from the Edit info button.

* Do not change the player without good reason (and admin approval).
* Follow the [Publisher Guidelines] regarding giving the proper Game Name, Version, Branch, ROM Name, and Movie filenames.
* The ""hacked game or otherwise impure movie"" checkbox places the movie in the Concept/Demos category.

----
See also:
* [Text Formatting Rules]
** [TextFormattingRules/ListOfModules|List of wiki modules]
* [User pages] (aka. homepages)
"
			},
			new WikiPage
			{
				PageName = "RecentChanges",
				RevisionMessage = InitialCreate,
				Markup =
@"[module:ActiveTab|tab=b6]

!!! Most recent changes in texts

Note: This only lists changes in text on this Wiki that are not minor edits.%%%
To see a list of new movie additions, go to the [new movies] page.%%%

*[=wiki.rss|RSS Feed]

[module:WikiTextChangeLog|limit=50|includetimes=Y]

----

[if:!CanEditPages]Do you want to edit pages? See [Users] to find out how.[endif]

If you're a Recent Changes junkie, you can try switching to [FullRecentChanges], which also lists minor edits.%%%
Movie maintenance edits are listed at [MovieMaintenanceLog]."
			},
			new WikiPage
			{
				PageName = "FullRecentChanges",
				RevisionMessage = InitialCreate,
				Markup =
@"[module:ActiveTab|tab=b6]

!! All recent changes in texts
This page is much like [recent changes] but it also lists minor edits.

[module:WikiTextChangeLog|limit=500|includeminors=1]"
			},
			new WikiPage
			{
				PageName = "SandBox",
				RevisionMessage = InitialCreate,
				Markup =
@"!! Welcome to the Sandbox

This is a page dedicated to experimenting with wiki markup.

By default any registered user can edit this page."
			},
			new WikiPage
			{
				PageName = "GameResources/CommonTricks",
				RevisionMessage = InitialCreate,
				Markup =
@"[module:listlanguages]

----

!!! Common tricks

''""This is a sparring program. Similar to the programmed''
''reality of the Matrix. It has the same basic rules – rules''
''like gravity. What you must learn is that these rules are no''
''different than the rules of a computer system. Some of''
''them can be bent. Others – can be broken.""'' ― Morpheus, The Matrix

There are several tricks that can be used (or at least tried)
in many games to improve the time.  If you are making a movie, be sure to try
every one of them. Otherwise, try [=forum/viewtopic.php?t=13010|finding your own].

%%TOC%%

!! Unintended things abuses

! Abusing untested code
''[TODO]: More about untested code.''

Untested code is a primary source of game glitches.

Untested code is not always helpful. For example, in [355M|Battletoads]
there's one level that is impossible to complete with two players because
the game testers never actually tried the level with two players.  The game was published containing a serious programming error that prevents the second player from moving (and resulting in his immediate death) in the Clinger-Wingers level.

! Abusing erroneous assumptions
Due to limited processing power, old console games often make
erroneous assumptions to reduce the number of calculations the
game has to make.  Sometimes these errors are just a result of
programmer oversight.

[module:youtube|v=baubmbgjzAg|start=64|loop=75|align=right|w=197|h=148|hidelink]
Many interesting examples exist, such as:
* In [1002M|Super Mario Bros 2], the games assumes that you are standing on an enemy if you are close enough to the top of it. So if you jump at the right spot next to an enemy, the ""ground flag"" is set and you can jump again in midair.
* The video on the right shows how in [1090M|Rygar], the game assumes you are standing on the ground if you are ducking, so if you walk and duck at small intervals you can walk off cliffs without falling down.
* In [1334M|Kid Chameleon] and [498M|Solomon's Key], the games assume you are standing on the ground every time a level starts, thus allowing you to do actions in midair at the start of a level, such as jumping or attacking.
* In [220M|Flashback], the game assumes you have collected items from previous levels even if that's not the case (due to warping). This also applies to most Prince of Persia games.

! Abusing side-effects of pause
[http://media.tasvideos.org/rockmantricks/pausetrick.gif|right]
In some games, pausing has unintended side-effects due to programmer oversight:
* In [1103M|Mega Man], pausing suspends bullets but keeps temporary invulnerability timers running, allowing shots to hit their targets multiple times if you pause while your target is being hit.
* The example to the right shows [1346M|Mega Man 2] where pausing resets vertical acceleration, which can be used to lessen the effects of gravity and perfom extra wide jumps.
* In some games, timers or counters continue to change during paused periods, which might allow certain types of luck manipulations. Sometimes it might even cause the timers or counters to overflow/underflow and lead to useful consequences.
* In most games, pausing creates a sound effect that momentarily disables a sound channel from music. If some scene is synchronized with the music, using pause may break the scene in some ways.

It might be worthwhile to observe what other anomalies are caused by
pausing in your game.

! Overlapping different game logics
* [http://www.youtube.com/watch?v=fmgDazOOxTY|Carring an enemy into a boss room], or [http://www.youtube.com/watch?v=TNCzboxA_ug|pulling a boss out to a regular level] may overlap some basic logics. RAM values, correct for each of them separately, mix with each other and sometimes give birth to new objects and routines.
* [http://www.youtube.com/watch?v=bOweqz4cDlA|Restarting a level] with some basic flags set and other ones (that should be set by common sense along with the first ones) unset sometimes also spawns wrong objects or gives access to the RAM.
* Bank switching abuse can glitch the normal NMI routines and use values from one game cicle to fill addresses ruling another cicle. This exactly happens in [1686M|RockMan 1 TAS] and calls Level End in a wrong time.

----
!! Monster damage abuses

! Monster ""damage boost"" abuse
It is common for games to be designed so that the
character can't reach certain paths or platforms, but just barely.
Designers often overlook the boost monsters provide when
they hit you. By getting hurt at the right place and the
right time, you can get monsters to boost you into places you normally
shouldn't be able to go.
* [901M|Castlevania] and [853M|Ghosts'n Goblins] have examples of this.

In some cases, taking damage sends the character in a direction neglecting obstructions due to oversight of the developers. This can be abused for creating unintended shortcuts.
* [721M|King Kong 2] has an example of taking damage to go through the gate to the final part of the game.

[module:youtube|v=deOhM2NNNGc|start=474|loop=480|align=right|w=197|h=148|hidelink]
In some cases, taking damage from powerful attacks flings the character at speeds faster than normal. If planned, this can send the character in the direction desired and save time. [126M|Super Castlevania 4], [868M|Legend of Zelda], [1122M|Link's Awakening], and [318M|Kirby Superstar] are examples of this.
* [1363M|Symphony of the Night] has examples of taking speed boost, height boost and distance boost resulted from damage.
%%%%
[module:youtube|v=CdPFvgKqPv4|start=66|loop=98|align=right|w=197|h=148|hidelink]
In some cases the damage causes the character to retain that speed as long as he doesn't stop.  In the video example here, [1304M|Adventure Island 4], the character is able to retain a super speed boost for a strikingly long period of time. See also [GameResources/CommonTricks#FrameWindowAbuse|frame window abuse].
%%%%
----
!! Invulnerability abuses

Most games contain some situations where your character is immune to harm.

! Event invulnerability abuse

Often, when the game has started a scripted demo that may
not be interrupted, like the animation for finishing the level,
your character becomes invulnerable.  If you touch something lethal
during this state of game, the game may very well ignore it.  The
[1000M|Little Nemo] and [1325M|Gremlins 2] movies have examples of this.
In many games,  such as [690M|Umihara Kawase] and [531M| The Wrath Of Black Manta], lethal
object collision checks are ignored when entering a door.  In [1346M|Mega Man 2], these checks are ignored when changing weapons.

! Stun invulnerability abuse

In many games, if you take damage you will flicker for a while and
temporarily become invulnerable, to prevent you from immediately
taking more damage. This invulnerability can often protect you
from ""instant death"" hazards, for example, allowing you to safely
walk on deadly spikes or lava for the duration of the flickering.
The [545M|Mega Man 4] and [365M|Blaster Master] movies have examples of this.

It may also be advantageous to use such a period of invulnerability to pass through monsters that would otherwise take longer to destroy, or
to pass through other nearby monsters that would do more damage.  The
[1051M|Goonies] movie is an example of the former, while [1093M|Castlevania: Circle of the Moon] movie is an example of the latter.

----
!! Avoid countdowns
Games often end levels with a score count down screen. The time spent on those screens count as well and should be minimized by causing the underlaying value to be as optimal as possible without losing speed. This usually means avoiding to collect items and having the correct amount of health at the end of the level.


!! Extraordinary gameplay

! Abusing death and Game Over
[module:youtube|v=RuVhJInysBQ|start=424|loop=450|align=right|w=197|h=148|hidelink]
While it may seem contradictory with goal to do things as fast as possible, it does not have to be. Games commonly reset the player position when the player dies, leading to possible shortcuts. Your health often also set to a known number and you may lose or gain items.

In games with multiple players, this can be used to change the amount of player interaction, for example dead players in an RPG will not get asked for their commands. Dead party members can also act as a form of party change, possibly affecting scripted events that depends on who is in the party.

In multi-player games, one player may be able to die to take advantage of a favorable spawning.  The video example to the right demonstrates this idea in Level 5 of [830M|Super C].  The two players continually die in order to spawn at the top of the screen to save time. 

This can all be abused to save time. Submission doing this are marked with the category [Movies-C3030Y|uses death to save time].

''[TODO]: abusing Game Over.''

! Avoiding saving

It often takes several seconds to manually save the game and is rarely needed in a TAS movie. Similarly, [=#AutoSave|turn off auto-saving].

However, in many games, you can take some advantage, such as to warp back to an earlier position in the level, reset character status, etc. when you load a saved game. It is recommended that you study what consequences can happen after saving the game.

''[TODO]: an example of taking advantage __just from saving__ the game?''

! Reset the game, keep the sram

Game consoles have reset buttons that restart the game from the beginning of the program. This may seem like an obvious suboptimal move, but at times it can be faster to reset the game after saving than continuing with the game. The most common example is letting the game save after finishing a level, but skipping the following cutscene.

! Reset the game, do mind the sram

Writing the save is a sensitive operation. As games often say ""Don't turn off the console"" during saving, ignoring this advice often leads to corrupted saves. Usually the result of a corrupted save is that the game refuses to load the save afterwards. But with careful timing and some luck it's possible to create a corrupt save that the game does accept loading. This can lead to major glitches and subsequent sequence breaking.

----
!! Input abuses
! Two or more actions at once

Try to make some input that most probably hasn't been tested, like pressing left+right or up+down at the same time, or use multiple input devices like gamepad+touchscreen simultaneously. It might have bizarre and useful consequences. See the [1210M|Zelda: ALttP] and [1082M|Zelda 2] glitch runs for examples of this.

(Note that some emulators (such as VirtuaNES or old versions of Snes9x and FCEU) don't allow you to press right+left or up+down.  The consoles themselves have no such qualms, though you usually have to press very hard on your controller to press in two opposite directions at once.)

* In [1626M|Shining Soul II], you can duplicate an item by tossing it and moving the item cursor at the same frame.

! Frame window abuse

In some games, there are short periods when you can make valid game input while you are not supposed to do so. Catching such an opportunity might either merely allow you to act earlier, or do things unintended by the developers.

* In [1222M|Castlevania: Harmony of Dissonance], you have can move right before you enter a room through an open door or a scripted event occurs. If you use backdash at certain doorway in such frame windows, glitched warp occurs.
* In [1410M|Blaster Master], you can turn around right when you are going through a doorway, which causes the game to warp you to the wrong destination.
* In [1700M|Pokémon Red], the game occasionally allows a short window where it is possible to open the menu during a cutscene, and using this opportunity to save and restore the game, or fly to a different area, can cause the game to get confused about what's going on, allowing memory corruption.

''[TODO]: Another typical example with a different outcome.''

! Alternating buttons

Often, you need to press a button to dispose of some dialog box, or to
start the game, or something similar. When two buttons can be used
to activate the same goal, you can press them like this:
 A, B, A, B, A, B, A, B, etc.
so that a button is effectively pressed every frame.
It doesn't save actual time in the movie, but it saves time making the movie, because you don't need to test for the earliest possible frame every time. Some emulators (such as VBA and Snes9x) let you put A and B on autofire starting at different frames to automatically press this sequence of buttons, which is even less effort for you.

Sometimes a game will actually accept the next input every frame, in which case it does actually save time in the movie compared to pressing one of the two buttons as early as possible each time. Also, some games might have been designed
not thinking it's possible to press buttons so fast, and strange things
may happen as a result.

Be aware that hexediting may be more cumbersome if you use this technique.
It may be difficult to determine the exact frame where the input affected
the game.

----

!! Super speed abuse

When you act inhumanly fast, like pressing a button 30 times
a second, you might be able to do things that the game
developers would never have thought possible.

* In some games, you can boost up your speed very high. See [1665M|Sonic Advance] for example.
* In some games, you can kill a boss while it's spawning, or perhaps before it has a chance to complete a transformation.  See [1119M|Simon's Quest], and [1075M|Super Metroid] for examples of this.

! Outrun the game
You can often catch the game unprepared if you move at extreme speeds. This means that the game may not have had the time to prepare itself for you being at that point. This means in practice that objects that should be there may not have been created yet. Depending on your exact situation this may be a blessing or a curse.

! Not on camera? Not there!
Games frequently cheat by only updating objects that are near enough to the camera. If you can somehow outrun the camera you might be able to ignore objects that would otherwise impede your progress.

! Fast motion collision abuse
[module:youtube|v=hETRzNzpFAI|start=205|loop=213|align=right|w=197|h=148|hidelink]
Games almost never interpolate motion between frames.  This means that you are in one place on one frame, and in some other place on the next, without actually having traveled the path between them, like we do in the real world.  This is how animation works.

If you are moving very fast, you can sometimes pass through objects
because the game does not render a frame when you are inside the object.
Combined with the close approach collision abuse mentioned above,
the speed may not even need to be very high.  

The example shows [711M|Gradius] collision abuse.  Due to the extreme speed, the ship is able to fly through narrow pieces of ground that would normally destroy the ship.
%%%%
----
!! Collision abuses
! Close approach collision abuse

Most games don't do pixel-perfect collision checks.  The character's and enemies' sprites (the graphics that you see on-screen) have unusual shapes, while their ""hit-boxes"" are usually actually square.  Games are programmed this way because it’s much easier to program ""is the character's box touching the enemy’s box"" than it is to program ""is any pixel of the character’s graphic touching any pixel of the enemy's graphic.""

In some games, the hit-boxes are actually bigger than the sprites, and you can get hit by an enemy before it seems like you should.  In a lot of games, though, the opposite is true: hit-boxes are actually smaller than the sprites, so you can seemingly touch or even go partially inside enemies without getting hurt.
The [1330M|SMB1], [1002M|SMB2], and [498M|Solomon's Key] movies have many examples of this.

! Diagonal movement between tiles

Some games check what tile character is on by just checking one point. This may make it possible to pass diagonally between tiles, sometimes useful to get past tiles that are deadly or otherwise harmful.

Example of this applied to Jetpack: [http://www.youtube.com/watch?v=EjTNYr7J_DY]

! Other collision abuses
[http://media.tasvideos.org/smbtricks/smb_minusentry2.gif|right]
By various methods (such as pushing into corners in strange ways or
getting shoved by an enemy) you may sometimes manage to get embedded
into the floor, walls, or ceiling.  Sometimes when this happens you just
get stuck and your game is ruined, but other times it might allow you to make use of new routes that weren't possible before.  For example, you might be able to gain support and jump from a wall.  The [1330M|SMB1] and [1031M|SMB2j] movies have examples of this.

In most games, collision calculations are very simple.  As mentioned earlier, because motion is not interpolated, sometimes you can go through enemies or walls if you are traveling fast enough or along an odd enough trajectory. With respect to walls, most games account for this by very quickly ejecting you from the wall to prevent you from getting stuck.  Sometimes, you can trick the game into ejecting you in the wrong direction, which is how walls are initially walked through in [1330M|SMB1].  In other games, wall ejection moves your character at high speed in one direction (usually horizontally) until the game
finds a place where you can exit the wall.  This is most useful when you can force the game to shoot you off at high speed along your desired path!
In [1103M|Mega Man] and [1346M|Mega Man 2], even ceilings eject you horizontally, which is why the authors of those movies often perform various antics that result in Mega Man being embedded in the ceiling.

The image on the right shows a classic example in Super Mario Bros (the famous wall glitch that allows entrance into -1)

----
!! Jumping off ledges

[http://media.tasvideos.org/smbtricks/smb_fallfast.gif|right]
The speed of playable characters in most games, especially platformers, is nonlinear. This is usually done in form of acceleration: the longer the character moves in a certain direction, the higher their velocity is. Similarly, the longer they fall, the faster they reach a platform underneath the starting point, so it's often preferable to jump shortly before the edge of the platform. Avoid the temptation of running off and falling normally, because often the character will have little-to-no downward velocity for a short period (unless, of course, you need it that way).

Note: Some games have drastically reduced aerial speed. For them, it's usually preferable to do very short jumps only a couple frames before reaching the ledge, or to not do any jumps at all. Be sure to study the physics of the game you're working on well, don't hesitate to use [memory search] to find exact values for character's speed.

----
!! Subpixel carryover

[module:youtube|v=JisFSMHn8tY|align=right|w=197|h=148|hidelink]
This is extremely common in NES games (and less common on platforms such as DS and GBA).  Regardless of platform it should be looked into for any game.

The subpixel value represents a fraction of a pixel.  It is used to calculate how many pixels a character moves in a frame.  Often at the end of a level/room, the remainder of this value is not cleared.  This means the value left over will be used on the next level/room.  When possible a high value should be left in a level to give the character a tiny ""head start"" in the next level.

Note: Subpixel variation is a common source of desyncs when copy/pasting movies.  In games with subpixel carryover, the subpixel values should be aligned before copy/pasting.

----
!! Luck manipulation

Video game consoles are computers.  They do exactly what they
are told, and will always give the same result when given the
same input.  There is nothing that is truly random.  The only
source of true randomness in games is you!
Games are purely deterministic and depend solely on user input.

Learn to abuse this fact.  You can affect anything that has an element of
randomness in it, like monster movements or item drops.

Read more at [Luck Manipulation].

----
!! Lag Reduction

The processing speed of old console systems is limited.
They can only handle so many objects per frame. If there are
too many objects on screen at one time, the game slows down, because it
just can't calculate everything in one frame.

Knowing the cause of this phenomenon is important when making
a TAS. Often this lag can be minimized by keeping
the number of on-screen objects low, either by destroying them early,
or perhaps ''not'' destroying them to prevent an overly complicated
(and processor intensive) death animation from playing.  Luck manipulation
can be useful here, too.  In some games, whether or not a monster spawns is
random.

On-screen objects are not the only cause of processor use. Some
actions require more calculation than others. For instance,
being close to enemies, or collecting or using certain items may
cause the game to lag.

Often lag can happen for unknown reasons, sometimes doing a slightly different strategy which doesn't cost time can prevent the event from occurring. In this instance a [memory search] might help to reduce it

----
!! Corrupt memory and save data
The game uses the memory to know what you have done, what you are doing and what you have left to do. If you somehow gain the ability to directly edit memory you have effectively won.

Slipping some wrong values into wrong addresses you can command a game to do arbitrary things. But that requires deep disassembling of the original game code, expert knowledge.

* [2187M|SRAM can be reprogrammed]
* [2157M|Execution pointer can be controlled]
* [1945M|Wrong values can be written to critical adresses]
* [1686M|Critical routines] [2047M|can be interrupted]
* Category: [Movies-C3058Y|Corrupts memory]
* Category: [Movies-C3057Y|Corrupts save data]

----
!! Game options
''[TODO]: Examples.''

Many games provide game options for you to modify how the games work. It is recommened always to try them out to see if you can save time with certain settings.

! Text speed
Some games allow you change how fast texts are displayed and fading out. Setting the text speed to the highest level usually saves time, and it is worth trying even if you can skip the texts, because sometimes you might be unable to skip them as quickly due to input limitations.

! Movement/battle animations
In some role-playing and strategy games, you can turn off or choose simplified versions of movement/battle animations. Choose the fastest way.

! Demos and cutscenes
Although you are likely to be able to skip them with input if the game provides such options, it might be faster to disable them just at all.

! Difficulty level
In general, we prefer movies done on the hardest difficulty provided that they demonstrate the best superhuman gameplay, but there are cases where lower difficulty may allow even better demonstration and we prefer that.

Note: In some games, you can access some extra items, stages, bosses and/or achieve some hidden endings if you are on a certain difficulty, which is usually the hardest one. Having this happening might interfere with your decided movie goals. So make sure that you have investigated the secrets in the game.

! Auto-save
It is usually a waste of time to save the game in a TAS, especially when it is automatically done everytime. Turn the auto-save option off if you are allowed to, otherwise, avoid triggering it as much as possible.

! Date and time
In many games, these settings affect the randomness in the games. See also [Luck Manipulation].

In some games, when the date and/or time matches with a particular value, some secret game material become accessible.

! User profile
Typically in many, but not limited to, long games featuring the save and load function, you are often asked to create a user profile before you can save, which usually contains some informations such as your or your game character's name, gender, birthday and so on.

It is usually a good idea to keep the informations as short as possible so that you can spend least time in creating the profile or having the informations cited during your gameplay. In some games, however, the informations in your profile may have deterministic or random effects on the gameplay and you may accordingly manipulate them to be in a favourable way.
----
!!! How to find out tricks

!! Study existing materials

Game walkthroughs, FAQs, forum posts, email letters, IRC remarks, etc. If you find or suspect that the game you are TASing has an engine similar to another game that has already had a TAS submitted, check it out to see if particular tricks or glitches used in it are applicable to your game as well.

!! Examine game data

! Read the program
All games are programs in some form. While they are intended to only be executed by the game system, there is nothing stopping you from reading the program instead. While the program code is most likely not very easy to read, it is the exact rules that the game is made out of.

The best way of understanding the rules is to simply read them and figure out exactly what they mean. You can check exactly how the [Luck Manipulation|random number generator] works or simply why the player character (supposedly) can not move into walls.

This is a very difficult thing to do, it is recommended that you know how to program before you attempt this. This is not normal programming, but on a lower level. You will need to actually understand the instructions that are given to the processor and not what the programmer may have written.

[Reverse Engineering]

! Read the script
Many games, especially ones with plot, have their own script engines. It commonly controls important events such as when you can go somewhere and how enemies behave.

Learn how the script engine works and examine what the game script does. It can have flaws just like the main program. But most importantly, it allows you to understand what your options are.

! Examine the level data
Games frequently store important data right inside the level definitions. You can find all sorts of tilemap flags, trigger areas, enemy controls and more! Why waste time guessing where exactly things are when you can simply look them up?

! Examine the data tables
Games frequently need to store large amounts of values for some purpose, be it the statistics for the items in the game, enemy health values, character statistic growth data or pre-computed math functions. There is no need to waste time hunting for values in memory when you can just read them all in one go.

!! Try things out
Simply try doing things that you aren't expected to do. Game developers forget about all the possibilities. It is your job to remind them what is actually possible."
			},
			new WikiPage
			{
				PageName = "GameResources/BossFightingGuide",
				RevisionMessage = InitialCreate,
				Markup =
@"%%%
Fighting perfectly against bosses can be very challenging for even 
the best TASers. The usage of the strategies listed below can significantly
improve a movie (in both frame count and entertainment value).

Note: These strategies are not restricted to boss fighting;
many of them can apply to other situations in games.

%%TOC%%

!!! Knowing
It is important to know what does it do to fight the boss before actually fighting it, especially for speedrunners. That could imply...

!! Avoid the unnecessary fight
In most situations the boss must be fought to make progress in the game, but sometimes it is not, and progress can be made without it. If the positive gain from fighting such an optional boss is not worth the time spent on it, then the fight might be unnecessary.

[TODO]: Examples (Castlevania II where the fight rewards almost nothing and some other RPGs).
----
!! Evade the supposed fight
Even if the fight was designed to be compulsory to make progress, it could be skipped with unexpected methods in some situations.

! Avoid the way where the boss gets in
In some games progress is made when the player character arrives at certain places, while the boss fight is just required to unlock the way to it. However, there may be some other way to reach the destinations. 

''Example:'' In [1144M|Metroid], Samus can access Tourian by luring a rio into the bridge room, freezing the rio, and using it as a floor.
----
! Avoid letting the boss spawn or activate
It may be possible to use a different route, pass through a wall, or
use another method that prevents the boss from obstructing where it should, or appearing at all.
From there, the character may be able to continue onward as if it had
already defeated the boss.

''Example 1:'' In [1054M|Maniac Mansion], if a kid is in the room with Purple Tentacle when a cutscene occurs, Purple Tentacle will become unable to move
afterward, and so the kid will be able to bypass it.

''Example 2:'' In [734M|Zanac], sprite load can cause some turrets not to spawn.

[TODO]: (Example 3: Torizo skip in Super Metroid. Is it also an example of the next trick?)
----
! See whether the character can advance to the next screen
[module:youtube|v=EewVM_G0YaU|start=1074|loop=1092|align=right|w=224|h=160|hidelink]
Sometimes it is incorrectly assumed that the boss must be defeated in
order to allow the character to advance to the next screen. If the
boss does not offer any useful item when it is defeated, see whether
the game’s programmers actually sealed the room’s exit.

''Example 1:'' The [962M|Glover] World 3 boss fight opens up a box containing the level-end item. It is possible to glitch into the box and skip the fight entirely.

''Example 2:'' In [1103M|Mega Man], the Cut Man rematch is avoided entirely by using the Magnet Beam glitch to zip into the next room.
----
! Replace the end-level trigger
Even if progress requires the game setting on a ""boss defeated"" flag or running a scripted post-fight event, there may be a way to set on the flag or trigger the event, or even to end the level directly without defeating it.

''Example:'' In the [355M|Battletoads] Rat Race level, killing any running rat
will cause the game to think that the boss has been defeated.
----
!! Fight only the boss
Although it may not be possible to avoid some bosses, there may be an
unorthodox way to do the opposite thing: to skip the level and quickly access the boss’s lair.

[TODO]: Example. (Little Samson?)
----
!! Skip to the last
Some games have a final lair that is normally accessible only
after defeating some bosses located at other places or require. To the full extent, there may be a way to skip to the final boss’s lair, or even the final goal behind it.

[TODO]: Example. (Zelda: A Link to the Past?)
----
!!! Planning
If the boss is decided to be fought, planning should be taken before carrying on.

!! Know what to do
! Know what the fight is all about
Sometimes the purpose of the boss fight is not to defeat
the boss. The character may need to press a button, accept a certain
amount of damage, or just stand up for enough time.

''Example 1:'' In [1270M|Super Metroid], the first fight against Ridley ends when Samus accepts a certain amount of damage. Therefore, the player’s goal should be
to lower Samus’s energy level as quickly as possible rather than to
fight against Ridley.

''Example 2:'' In [549M|Final Fantasy VI], after Vargas uses his Blizzard Fist technique, the usage of Blitz Pummel will start a cutscene in which Vargas resigns the fight — ending the battle much more quickly than defeating him conventionally.
----
! Have the correct item
The boss may be invulnerable or impassible without a particular item.
Know which item, if any, is necessary in order to overcome the boss,
and know whether the item is ''really'' necessary.
----
!! Plan further
The boss fight is not everything. More than the fight itself should be considered.

! Minimize time-consuming weapon/item switching
Sometimes, bosses are susceptible to different weapons or items. If switching takes considerable time (e.g., the player has to pause the game in order to bring up the weapon menu), then it can be quicker to stick to one effective weapon for two or more bosses rather than to select the most effective weapon each time.

If the player is free to decide the order of the boss fights, then choosing the optimal order can help reduce weapon-switching.
----
! Minimize automated demos after the fight
Sometimes defeating the boss will cause a scene where the character must
wait. If possible, this delay should be avoided.

''Example:'' In [1349M|Super Mario Bros. ""warpless""], it is faster to use fireballs
on the bosses in order to prevent the slow bridge-destruction scene.
----
! Minimize end-level inventories
In many games, the player receives bonus points for things like health, weapon power, or time. Sometimes it is advantageous to do things like lose health or fire ammo in order to minimize the time that it takes to tabulate these bonuses. Conversely, in some games, instead of receiving a bonus, the game takes time to refill the character after the boss fight. In this case, the player should optimally balance the time that it takes to refill with the speed of the boss fight.

''Example:'' In [467M|Kabuki - Quantum Fighter] the character can take damage during the boss fight without losing time. Thus, there is less of a health bonus.
----
! Get in the best position after the fight
If an exit appears after a fight, position the character so that it can
go through the exit as early as possible. If the character must accelerate from a complete stop, then have the character already moving at its fastest speed. It may be a good idea to draw the boss toward the exit during the fight. If the boss drops an important item after the fight, make the character grab it as early as possible.

''Example:'' In [1103M|Mega Man], the boss releases its weapon upgrade.
Mega Man should be positioned to grab the upgrade as soon as it appears.
----
! Get the boss in the ideal position on the last hit
The boss's death (e.g., exploding into debris) can cause lag.
Having the boss at the edge of the screen can minimize this lag. Alternatively, the debris of the boss may have to leave the screen entirely in order to progress, and so the boss should be in the center of the screen.

''Example 1:'' In Mega Man [476M|5] & [1241M|6], it is optimal to have the boss in one of the far corners of the room so that some of the debris leaves as soon as possible.

''Example 2:'' In Mega Man [545M|4] & [776M|7], the debris from the boss must clear the room before Megaman can get the power-up. In this case, the boss should be in the center of the screen.
----
!!! Fighting
!! Attack early
! Start the fight early
There can be various trigger conditions of a boss fight. It could be to scroll the screen, move the character to a certain position, or wait for a timer to count to a specific value. This can be an opportunity for optimizations.

TODO: Examples.
----
! Get in the best position when the fight starts
While seemingly contrary to the previous trick, it can be more advantageous to spend time in putting the character in a more ideal position before the fight begins than starts it as early as possible.

''Example:'' In [1346M|Mega Man 2], it is possible to zip right up next to the boss at the beginning of the fight.
----
! Attack as early as possible
Fire a long-range weapon as early into the battle as possible, as long as the projectile will damage the boss. This moment could be the first frame on which the
character can use its weapon after a delay (e.g., dialogue). In a side-scroller, the moment could be just before the boss appears on the screen, because the character — after discharging its weapon — will scroll the screen when it continues moving toward the boss. After the first shot, make the character continuously attack the boss on the earliest frame that the boss will accept damage.
----
! Hit the boss before the fight begins
[module:youtube|v=-JRncSed9hw|start=180|loop=194|align=left|w=224|h=160|hidelink]
[module:youtube|v=EByv1XTLo5Y|start=528|loop=538|align=right|w=224|h=160|hidelink]
In some cases, it might be possible to hit the boss before it seems possible.%%%

In ''Example 1'' on the left: [425M|Cobra Triangle], the player's boat can fire a shot just before it lands in each level. This extra shot can be used to hit the boss before the boss fight even starts.

In ''Example 2'' on the right: [776M|Mega Man 7], the junk shield weapon can be used in the previous screen in order to hit Freeze Man during the beginning boss cutscene!

Some bosses are glitchy and will accept damage before they visually
appear on the screen.

''Example 3:'' In [1265M|Power Blade], many of the bosses
can accept damage before the screen loads, while they have 0 HP — entailing
instant death. 

''Example 4:'' In [1119M|Castlevania 2], the final boss can accept damage while it materializes onto the screen.
----
!! Attack dominantly
! Affect the boss’s behavior
In some games, the boss may react to the character’s actions.
Performing certain actions may prevent the boss from doing undesirable
things such as jumping, becoming invulnerable, or performing an attack
that takes time to avoid.
----
! Make the boss commit suicide
See whether the character can trick the boss into falling into a
hole or spike pit or otherwise damaging itself.

''Example:'' In [485M|Double Dragon], it is possible to knock Abobo onto a conveyor belt that takes him into a hole. Later in the movie, Willy is manipulated into shooting himself with his own gun.
----
! Constrain the boss
[module:youtube|v=CNHw2zwJKBE|start=106|loop=119|align=left|w=224|h=160|hidelink]
Although the boss should ''look'' menacing, search for a method that greatly conatrains its ability and decisively defeats it. This is usually achieved utilizing some hit-stun or knock-back design on the boss, but some unorthodox way may also work.

''Example 1:'' In [1443M|Metroid], Samus can stun the mini-bosses by repeatedly shooting them at close range.

''Example 2:'' In [1168M|Circle of the Moon], Dracula in the Sealed Room is standing still and open to attack when Nathan enters the room in a glitched way.
----
!! Attack effectively
Avoid attacks that do not count, either miss or get blocked, unless it has a purpose eg. to manipulate randomness.

! Find the weak spot
Sometimes particular sections of the boss may not accept damage or may
accept damage but not be essential to the death of the boss. The player
should know where an attack on the boss will actually contribute to the
death of the boss.

''Example:'' In [853M|Ghosts ’n Goblins], the dragon’s tail
can be destroyed, but only the attacks on its head will cause its demise.
----
! Know for how long the boss’s invulnerability lasts
[module:youtube|v=s760jW7HnZc|start=132|loop=175|align=right|w=224|h=160|hidelink]
The character should not attack the boss when it will not cause any damage, such as (sometimes) at the start of a battle or if the boss has a recovery period after the character does damage to it. Make sure that when the invulnerability wears off, the attack ''damages'' the boss on the first possible frame. This timing entails that the character may need to start charging the weapon while the boss is still invulnerable.

If there is a visual indication of when the boss becomes vulnerable again, such as that it stops blinking, make sure to deliver the next attack(s) on (or, possibly, before) the frame on which the indication begins, lest it become obvious that the character did not attack the boss as soon as possible. If there is a sound that plays whenever the character’s attack does damage or whenever the boss accepts damage, then the player should listen for that sound, lest it become obvious that some attacks were mistimed.

In this clip from [1103M|Mega Man] shows how bullets land on the first frame the boss is vulnerable.  Notice how you can hear shots land at a regular interval.
----
! Watch HP in memory
In some games, bosses will take an unexpected amount of damage if they are hit
at a certain time or from a certain position. Also, it may be unclear which
weapon is the strongest against the boss. If the game does not display the
boss’s exact HP value onscreen, then watching the value in memory while trying different techniques may yield a way to deal more damage than normally possible and/or determine which weapon is the best to use.

''Example 1:'' Many bosses in [1067S|Pulseman] can take 10–12 damage per hit under the right circumstances, whereas a normal hit does only 2 damage.

''Example 2:'' In [830M|Super C], the level 5 boss appears to take damage as it descends, but no damage is registered on the HP address. Worse yet, it will take longer for the boss to become vulnerable if the character shoots it during this time. HP watching may reveal counterintuitive results!
----
!! Attack efficiently
! Find the boss’s weakness
[module:youtube|v=LcDhcpvqd9o|start=40|loop=52|align=left|w=224|h=160|hidelink]
[module:youtube|v=NOyhnLe2Ht8|start=1362|loop=1367|align=right|w=224|h=160|hidelink]
The character may be able to use certain weapons that hurt the boss much
more than other weapons would. These weapons can be either power-ups
or elemental-based. For example, a fire-based boss may be weak against
a water-based weapon.

In [562M|Ninja Gaiden], the Jump-and-Slash technique is always better than the normal technique.

In Mega Man games, bosses are almost always weakest to a particular weapon.  The example on the right, [1346M|Mega man 2] shows a striking example of finding a boss's weakness.
----
! Find ways to increase the effectiveness of an attack
In some cases, weapons do more damage only when certain conditions have been met.

''Example:'' In [1593M|TMNT], the character does more damage if its health is under 50%.  Thus, the character should lose health before the boss fight.
----
! Use critical hits
[module:youtube|v=h1i-JD7ivgc|start=410|loop=437|align=left|w=224|h=160|hidelink]
[module:youtube|v=h1ZwXC4siKU|start=12|loop=27|align=right|w=224|h=160|hidelink]
Against bosses in RPGs, the fight probably will be faster if the player
manipulates randomness in order to make the character deliver critical hits
instead of normal hits. This randomness may be based on the frame on which the attack is selected or the buttons pressed during menu navigation (or a combination of both).

In the example on the left, [519M|Dragon Warrior 4], The character manipulates nothing but critical hits against a boss (and manipulates the boss to miss with each of its attacks).

Critical Hits aren't limited to RPG movies either.  The example on the right shows a boss fight from [1222M|Castlevania: Harmony of Dissonance] where the character delivers many critical hits in a very short time frame.
----
! Pause the game
[module:youtube|v=s760jW7HnZc|start=690|loop=705|align=right|w=224|h=160|hidelink]
The boss may continue to accept damage while the game is paused, or it may
accept damage repeatedly as the game is paused and unpaused.

On the right is the most famous example of this, the ""pause glitch"" in [1103M|Mega Man].  Continuously pausing and unpausing can cause one projectile to damage the boss multiple times.
----
! Use the character as a weapon
In rare cases, the character's physical location can affect the damage dealt to the boss. One possible method for using the character as a weapon is to make
the character accept damage from the boss on the same frame that the
character’s weapon hits the boss.

''Example:'' Many bosses in [901M|Castlevania] can accept critical damage from
the character if they are hit on the correct frame.
----
!! Attack quickly
! Use the fastest method of attack
[module:youtube|v=6Y666I8W5B4|start=870|loop=885|align=right|w=224|h=160|hidelink]
Additionally, if a certain attack is faster than another attack, it is
probably wiser to use the quicker attack, unless the quicker attack is
so weak that it ultimately entails a longer fight. 

In general, the player should choose whichever attack has the highest
ratio of damage to time. If the boss has a long invulnerability period
after it takes damage, then the speed of each attack that the character
can perform is effectively reduced to the speed at which the boss becomes
vulnerable; in this case, stronger-but-slower attacks may kill the boss more
quickly than faster-but-weaker attacks. The player should know how much
damage ''each'' type of attack does to the boss (e.g., how many bars of
health an attack reduces and/or how many attacks are needed to kill it).

In the example on the right, [1114M|Zelda II], it is much quicker (and more impressive) to dance on top of the bosses and use downstab than it would be to repeatedly jump and swing.
----
! Move near the boss
During the firepower output period, do not position the character at sniping distance, because attacks will take longer to hit the boss. Also, in many games the character can
discharge its weapon (e.g., bullets) only after the previously fired
projectile has disappeared from the screen. Thus, moving next to the
boss allows the character to attack more frequently. Sometimes the character
can go inside the boss (by abusing temporary invulnerability
after being attacked) in order to attack even faster. Some projectiles
are not absorbed when they damage the boss; in this case it may be
more efficient to move closer to and to face the edge of the screen so that
the projectiles from the character’s weapon go offscreen faster.
----
! Jump with the boss
Even if the character has a weapon that can shoot upward, the character
probably should stay as close to the boss as possible when it jumps.
In addition to being more efficient, aerial fighting can offer the
''Matrix''-like fighting that is sought in TAS movies.
----
! Find a safe zone
Without conflicting with the ‘be adjacent’ aspect, find a safe area.
If the character must dodge the boss’s projectiles or body, then the
character may need to jump or step back. This dodging entails that
the character must stop attacking until it reestablishes its position
and the dangerous objects have cleared out.
----
! Take damage
In some games, it is faster
to stand in the way and keep attacking while accepting damage than it is
to avoid the boss’s attacks; the player should use this tactic only if it
is the fastest possible method (i.e., do not be lazy).
----
! Double-team
[module:youtube|v=EkfEF8ykaUo|start=505|loop=526|align=right|w=197|h=148|hidelink]
Some games have a ‘two-player simultaneous play’ option. Two players often
can destroy the boss quicker than one player alone could.

''Example:'' In [499M|Rush ’n Attack], the two players can cover both sides of
the screen, use teamwork, etc.

In the video example to the right, 2-players are used in [1279M|Double Dragon 2] to toss the final boss back and forth."
			},
			new WikiPage
			{
				PageName = "System/GameResourcesFooter",
				RevisionMessage = InitialCreate,
				Markup =
@"----
!!! See Also
* [GameResources|Game Resources] - resource pages for specific games.
* [CommonTricks|Common Tricks] - tricks common to many games."
			}
		};
	}
}
