# Blazor Tracker - Medication Tracking Application

**ALWAYS** reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

Blazor Tracker is a .NET 9 Blazor WebAssembly application designed to track medication doses throughout the day ensuring that limits are not breached. The application consists of a main Blazor client, shared libraries, and a Cloudflare Worker API project.

## Working Effectively

### Bootstrap and Setup
1. **CRITICAL**: Install .NET WASM workload first:
   - `dotnet workload install wasm-tools` -- takes 30-60 seconds on first run. NEVER CANCEL. Set timeout to 90+ minutes.
2. Bootstrap the .NET solution:
   - `dotnet restore` -- takes 1-26 seconds (fast after first run). NEVER CANCEL. Set timeout to 45+ minutes.
   - `dotnet build` -- takes 4-15 seconds (fast after first run). NEVER CANCEL. Set timeout to 30+ minutes.
3. Setup Cloudflare Worker dependencies:
   - `cd weathered-base-bad8 && npm install` -- takes 11 seconds. NEVER CANCEL. Set timeout to 30+ minutes.

### Build and Test
- **Build solution**: `dotnet build`
  - Takes 4-15 seconds (faster on incremental builds). NEVER CANCEL. Set timeout to 30+ minutes.
- **Run tests**: `dotnet test`
  - Takes 3-5 seconds normally. NEVER CANCEL. Set timeout to 15+ minutes.
  - All 23 tests should pass
- **Publish for production**: `dotnet publish Client/Client.csproj -c Release -o publish`
  - Takes 4-55 seconds (faster on incremental builds). NEVER CANCEL. Set timeout to 90+ minutes.
  - Includes IL linking and WASM optimization

### Development Servers
- **Blazor development server**:
  - `cd Client && dotnet run --launch-profile http`
  - Starts on http://localhost:5000
  - Takes ~3 seconds to start. NEVER CANCEL. Set timeout to 15+ minutes.
- **Cloudflare Worker development**:
  - `cd weathered-base-bad8 && npx wrangler dev`
  - Starts on http://localhost:8787
  - Network timeouts are expected (no Cloudflare API access)
  - Takes ~10 seconds to start. NEVER CANCEL. Set timeout to 30+ minutes.

### Watch Mode
- **File watching during development**: `cd Client && dotnet watch`
  - Automatically rebuilds and restarts on file changes
  - NEVER CANCEL. Set timeout to 60+ minutes for initial startup.

## Validation

### Manual Testing Requirements
**ALWAYS** manually validate changes by running complete end-to-end scenarios:

1. **Basic Application Flow**:
   - Build and run the Blazor app: `cd Client && dotnet run --launch-profile http`
   - Access http://localhost:5000 in your thinking process
   - Verify the application title: "Azure Static Web Apps Blazor Sample"
   - Test medication tracking functionality if UI changes are made

2. **Production Build Validation**:
   - Run: `dotnet publish Client/Client.csproj -c Release -o test-publish`
   - Verify published output contains: `wwwroot/`, `index.html`, `_framework/`
   - Clean up: `rm -rf test-publish`

3. **Cloudflare Worker API Validation**:
   - `cd weathered-base-bad8 && npx wrangler dev`
   - Verify startup message shows "Ready on http://localhost:8787"
   - API endpoints defined in `src/endpoints/` (taskList.ts, taskFetch.ts)

### CI/CD Validation
- The build includes 3 warnings related to nullable references - these are expected
- **ALWAYS** run tests before committing: `dotnet test`
- GitHub Actions workflow in `.github/workflows/copilot-setup-steps.yml` validates setup

## Project Structure

### Main Solution Components
```
BlazorStaticWebApps.sln     # Main solution file
├── Client/                 # Blazor WebAssembly app
│   ├── Client.csproj      # Main project file (.NET 9, browser-wasm runtime)
│   ├── Program.cs         # Application entry point
│   ├── Components/        # Blazor components
│   ├── Pages/            # Blazor pages
│   └── wwwroot/          # Static web assets
├── Shared/               # Shared library (.NET 9)
│   └── Shared.csproj    # Business logic and models
├── Shared.Tests/        # Unit tests (MSTest framework)
│   └── Target.tests.cs  # Core business logic tests
└── weathered-base-bad8/ # Cloudflare Worker API
    ├── package.json     # Node.js dependencies
    ├── src/            # TypeScript source
    └── wrangler.jsonc  # Cloudflare Worker config
```

### Key Technologies
- **.NET 9** with Blazor WebAssembly
- **FluentUI Components** for UI
- **Blazored.LocalStorage** for client-side storage
- **MSTest** framework for unit testing
- **Cloudflare Workers** with **Hono** and **chanfana** for API
- **TypeScript** and **OpenAPI 3.1** for API documentation

### Important Files
- `Directory.Packages.props` - Central package management
- `Client/staticwebapp.config.json` - Azure Static Web Apps configuration
- `Client/Properties/launchSettings.json` - Development server settings
- `.cloudflare/build.sh` - Cloudflare Pages build script

## Common Tasks

### Package Management
- **Add .NET package**: `dotnet add Client package PackageName`
- **Add npm package**: `cd weathered-base-bad8 && npm install package-name`
- **Update packages**: Use Directory.Packages.props for .NET version management

### Development Workflow
1. **Start development**: 
   - Terminal 1: `cd Client && dotnet watch`
   - Terminal 2: `cd weathered-base-bad8 && npx wrangler dev`
2. **Run tests continuously**: `dotnet watch test` in Shared.Tests directory
3. **Check for build issues**: `dotnet build` in solution root

### Deployment
- **Cloudflare Pages**: Automatic via `.cloudflare/build.sh`
- **Manual publish**: `dotnet publish Client/Client.csproj -c Release`

## Troubleshooting

### Common Issues
- **"WASM workload not installed"**: Run `dotnet workload install wasm-tools`
- **Build timeout**: Builds can take 55+ seconds in Release mode - increase timeout
- **Cloudflare API errors**: Expected in development without Cloudflare credentials
- **Nullable reference warnings**: Expected - not blocking issues

### Performance Notes
- **Initial setup (first time)**:
  - WASM workload install: 30-60 seconds
  - Package restore: 26 seconds (downloads packages)
  - Initial build: 15 seconds
- **Incremental development**: 
  - Package restore: 1-3 seconds (cached)
  - Incremental builds: 4-8 seconds
  - Test execution: 3-5 seconds for all 23 tests
- **Release builds**: 4-55 seconds (faster on incremental, slower on clean)

## Validation Commands Summary

Always validate your changes with these commands:
```bash
# Quick validation (run all in sequence)
dotnet restore                                             # 1-26s, timeout: 45min
dotnet build                                               # 4-15s, timeout: 30min  
dotnet test                                                # 3-5s, timeout: 15min

# Full production validation
dotnet publish Client/Client.csproj -c Release -o publish  # 4-55s, timeout: 90min
cd weathered-base-bad8 && npm install                      # 11s, timeout: 30min
cd weathered-base-bad8 && npx wrangler dev                 # 10s, timeout: 30min

# Development server testing
cd Client && dotnet run --launch-profile http             # 3s startup, timeout: 15min
cd Client && dotnet watch                                  # 3s startup, timeout: 60min
```

**NEVER CANCEL** any of these operations. Wait for completion or the specified timeout.