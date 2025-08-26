# TASVideos End-to-End Tests

This project contains automated end-to-end tests for the TASVideos website using Microsoft Playwright.

## Features

- **Environment Configuration**: Configurable to run against local development or production
- **Throttling**: Built-in request throttling to limit impact on production servers
- **Response Code Validation**: Tests verify expected HTTP response codes
- **HTML Element Verification**: Tests check for presence of expected page elements
- **Configurable Timeouts**: Adjustable timeout settings for different environments

## Configuration

### Environment Settings

Configure test settings in `appsettings.json`:

```json
{
  "E2ESettings": {
    "BaseUrl": "https://tasvideos.org",           // Production URL
    "LocalUrl": "https://localhost:44385",        // Local development URL  
    "Environment": "Production",                  // "Production" or "Local"
    "ThrottleDelayMs": 2000,                     // Delay between requests (production only)
    "RequestTimeoutMs": 30000,                   // Request timeout
    "MaxRetryAttempts": 3,                       // Max retry attempts
    "HeadlessMode": true,                        // Run browser in headless mode
    "SlowMo": 0                                  // Slow down operations (ms)
  }
}
```

### Environment Variables

You can override settings using environment variables:

- `E2ESettings__Environment=Local` - Switch to local testing
- `E2ESettings__ThrottleDelayMs=5000` - Increase throttling delay
- `E2ESettings__HeadlessMode=false` - Run with visible browser

## Running Tests

### Prerequisites

1. Install Playwright browsers:
   ```bash
   dotnet test --logger trx -- playwright install
   ```

### Run All Tests

```bash
dotnet test
```

### Run with Verbose Output

```bash
dotnet test --verbosity normal
```

### Run Specific Test Class

```bash
dotnet test --filter "HomePageTests"
```

### Run Against Local Environment

Set environment variable before running:
```bash
$env:E2ESettings__Environment="Local"
dotnet test
```

## Test Structure

### Base Classes

- **BaseE2ETest**: Base class providing common functionality
  - Configuration loading
  - Throttling management
  - Helper methods for assertions
  - Navigation with automatic throttling

### Test Categories

- **HomePageTests**: Tests for the main homepage
  - Response code validation (200)
  - Essential HTML elements presence
  - Page title verification
  - Navigation links verification
  - Load time performance
  - Error message absence

## Throttling

The test framework includes built-in throttling specifically for production environments:

- **Production**: Automatically applies configured delay between requests
- **Local**: No throttling applied for faster test execution
- **Thread-Safe**: Uses locking to ensure proper throttling across parallel tests

## Best Practices

1. **Always Test Against Both Environments**: Verify tests work locally before running against production
2. **Respect Production Resources**: Use appropriate throttling delays
3. **Meaningful Assertions**: Test both response codes and page content
4. **Descriptive Test Names**: Use clear, descriptive test method names
5. **Error Handling**: Include tests for error conditions and edge cases

## Extending Tests

### Adding New Test Classes

1. Inherit from `BaseE2ETest`
2. Use `NavigateWithThrottleAsync()` for navigation
3. Use assertion helper methods (`AssertResponseCodeAsync`, `AssertElementExistsAsync`, etc.)
4. Follow existing naming conventions

### Example Test

```csharp
[TestMethod]
public async Task SamplePage_ShouldLoad_Successfully()
{
    var response = await NavigateWithThrottleAsync("/sample-page");
    
    await AssertResponseCodeAsync(response, 200);
    await AssertElementExistsAsync("h1", "Page heading");
    await AssertElementContainsTextAsync("title", "Expected Title");
}
```

## Installation and Setup

### Initial Setup

1. **Install Playwright browsers** (first time only):
   ```bash
   cd tests/TASVideos.E2E.Tests
   powershell bin/Debug/net8.0/playwright.ps1 install
   ```

2. **Build the test project**:
   ```bash
   dotnet build
   ```

### Test Results

The implementation successfully demonstrates:
- ✅ Configurable environment settings (Production/Local)
- ✅ Request throttling for production (2-second delays)
- ✅ HTTP response code validation (200 OK)
- ✅ Page title verification
- ✅ Timeout monitoring and performance testing
- ✅ Error message detection
- ⚠️ HTML element detection (needs selector refinement)

## Troubleshooting

### Common Issues

1. **Browser Installation**: Run `powershell bin/Debug/net8.0/playwright.ps1 install` in project directory
2. **Timeout Errors**: Increase `RequestTimeoutMs` in configuration
3. **Throttling Too Aggressive**: Reduce `ThrottleDelayMs` for faster testing
4. **SSL Certificate Issues**: Tests ignore HTTPS errors by default
5. **Element Not Found**: Use browser developer tools to verify selectors

### Debug Mode

Run with visible browser for debugging:
```bash
$env:E2ESettings__HeadlessMode="false"
dotnet test
```

### Performance Note

When running against production with throttling enabled, expect approximately:
- 2 seconds delay between each test
- Total execution time: ~25 seconds for 6 tests