# Contributing to the TASVideos Site

Welcome! The TASVideos staff have set up links here to help guide new developers toward contributing to the code behind the TASVideos website.

## Directing Contributions

Please feel free to file issues for any bugs observed! The maintainers will aim to confirm and mark them with a milestone based on their severity. We also welcome creating PRs directly if the solution is readily available and a maintainer will review the pull request and merge it. Developers who submit more complex or frequent PRs are encouraged to join the [site discord](https://tasvideos.org/LiveChat) and ask for a site developer role. Any contributions will be [licensed under GPL v3](LICENSE).

If a security issue is found please instead follow our [security policy](SECURITY.md) to disclose the issue.

## Code of Conduct

Contributors are expected to uphold a basic [code of conduct](CODE_OF_CONDUCT.md) in all interactions inside and outside of the project.

## Developer Setup

See [Local Development Setup](https://github.com/TASVideos/tasvideos/wiki/Local-Development-Setup) in the readme for instructions on running the site locally for development.

There are several test suites in the `/tests` directory, which you can run in the conventional way (`dotnet test [TASVideos.sln]`).
They are also run in CI.
If you want to check coverage locally, it's `dotnet test --collect:'Code Coverage' && dotnet dotnet-coverage merge 'tests/*/TestResults/**/*.coverage' --output-format=cobertura --output=merged.coverage.xml`.

## Code Style

The codebase uses the [Allman style](https://en.wikipedia.org/wiki/Indentation_style#Allman_style), placing braces on their own line. It uses tabs for indentation including in HTML in CSHTML where it acts as a single space when parsed by web browsers. Otherwise, code style should generally follow conventions for .NET 5 and C#.

Most of our code style rules are configured in EditorConfig, so you can use `dotnet format` or any other Roslyn-powered linter to apply them.
We're down to 0 warnings, and let's keep it that way!

If possible, please include unit tests when adding new features.
We don't have Selenium or anything set up so you can't really test the frontend automatically, but any increase in test coverage is welcomed.

## Site Design

The site has a [design document](DESIGN-SPEC.md) which details the structure, philosophy, and design goals of the frontend segments of the codebase which is great study for aspiring frontend contributors.
