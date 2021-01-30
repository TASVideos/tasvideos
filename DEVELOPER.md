# Prerequisites
* .NET Core 5.0 SDK
* VSCode or VS recommended for best development experience
* MSSQL Database for local development
# Building
* `dotnet build` in the solution directory
# Running
* You'll need a database, first.  Add the connection string to it:
	* `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "MyConnectionString"`
* `dotnet run` in the TASVideos project will run the website server.
* In VSCode, launch.json is set up with a `"TASVideos"` launchable that sets all the right parameters.
* In VS, ???
# Testing
* `dotnet test` in the solution directory
