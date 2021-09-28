![.NET Core](https://github.com/adelikat/tasvideos/workflows/.NET%20Core/badge.svg)

# TASVideos 2.0
This is a rewrite attempt of the tasvideos website.

It aims to change technologies, have simpler more maintainable code, and bring the site up to modern standards.

Goals of this rewrite include:
- Ability to run as Https/SSL (http redirects to https)
- Address multi-lingual issues
- Responsive design/mobile friendly
- Ability to be a Site Admin without db access or coding experience
- Maintainable
- Architectured in a way that can facilitate major site changes as the needs of the community change
- Ability to run on Linux in production
- Ability to run on any OS in a local development environment
- Feature parity
- Ability to host GDQ TAS content
- Ability to store and facilitate console verification knowledge and verification data/files

A demo server has been set up for this project:
https://tasvideos.mistflux.net/

This demo updates with production days once a day, and updates with the latest code twice a day.

# Local Development set up

## Windows
* Install latest Visual Studio (Community 2019) from https://visualstudio.microsoft.com/
* Run Visual Studio installer
* Select the followings:
** ASP.NET and web development
** .NET desktop development
* Clone repository from https://github.com/adelikat/tasvideos.git
* Open TASVideos.sln
* Select profile "Dev MsSql (Sample Data)"
** Note that once you run once with Sample Data, the profile "Dev MySql (No Recreate)" can be used for quicker start up times
*** Optionally
* Install the latest postgres and use the "Dev Postgres" profiles instead.  This will better mimic deployed environments (which will always use postgres)

## Mac
* Install latest Visual Studio for Mac
* Install postgres
* Install .net 5 (unless already installed from Visual Studio)
* Clone repository from https://github.com/adelikat/tasvideos.git
* Open TASVideos.sln
* Select profile "Dev Postgres (Sample Data)"
** Note that once you run once with Sample Data, the profile "Dev Postgres (No Recreate)" can be used for quicker start up times

## Linux
* Install VSCode
* Install postgres
* Install .net 5
* Clone repository from https://github.com/adelikat/tasvideos.git
* TODO: better document how to pick the postgres Sample Data profile
