![.NET Core](https://github.com/TASVideos/tasvideos/workflows/.NET%20Core/badge.svg)

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
https://demo.tasvideos.org/

This demo updates with production days once a day, and updates with the latest code twice a day.

# Local Development Setup

## Prerequisites
- .NET Core 5.0 SDK
- VSCode or VS recommended for best development experience
- MSSQL from VS or postgres installed for the local database

## Quick-Start Instructions

### Visual Studio (Windows)
- Install Visual Studio Community 2019 from https://visualstudio.microsoft.com/
- Run Visual Studio installer
- Select the following:
	- ASP.NET and web development
	- .NET desktop development
- Clone repository from https://github.com/TASVideos/tasvideos.git
- Open TASVideos.sln
- For MSSQL
	- Select profile "Dev MsSql (Sample Data)"
		- Note that once you run once with Sample Data, the profile "Dev MySql (No Recreate)" can be used for quicker start up times
- For postgres
	- Use the "Dev Postgres" profiles instead.  This will better mimic deployed environments (which will always use postgres)

### Visual Studio (macOS)
- Clone repository from https://github.com/TASVideos/tasvideos.git
- Open TASVideos.sln
- Select profile "Dev Postgres (Sample Data)"
	- Note that once you run once with Sample Data, the profile "Dev Postgres (No Recreate)" can be used for quicker start up times

### VSCode (Windows, macOS, Linux)
- Install VSCode from https://code.visualstudio.com/
- Install .NET 5.0 SDK from https://dotnet.microsoft.com/download
- Clone repository from https://github.com/TASVideos/tasvideos.git or download and extract https://github.com/TASVideos/tasvideos/archive/refs/heads/master.zip
- Open the folder of repository in VSCode (required to make sure your current working directory is set to make the predefined launch.json and tasks.json to work)
- Install C# for Visual Studio Code (pops up after visiting your local tasvideos folder)
- Select TASVideos to run first
- TODO: Document database setup and elaborate how to choose the right thing in launch.json
