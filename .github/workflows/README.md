# GitHub Actions Workflows

## Release Avalonia Desktop

Automated build and release workflow for the SubdomainScanner Avalonia desktop application.

### Trigger Methods

1. **Tag Push** (Recommended):
   ```bash
   # Create and push a version tag
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Manual Trigger**:
   - Go to GitHub Actions tab
   - Select "Release Avalonia Desktop" workflow
   - Click "Run workflow"
   - Choose branch and click "Run workflow"

### Workflow Details

#### Build Jobs

The workflow builds the application for multiple platforms in parallel:

1. **Windows x64**
   - Runner: `windows-latest`
   - Output: `SubdomainScanner-win-x64.zip`
   - Self-contained with .NET 9.0 runtime

2. **Linux x64**
   - Runner: `ubuntu-latest`
   - Output: `SubdomainScanner-linux-x64.tar.gz`
   - Self-contained with .NET 9.0 runtime

3. **macOS x64** (Intel)
   - Runner: `macos-latest`
   - Output: `SubdomainScanner-macos-x64.tar.gz`
   - Self-contained with .NET 9.0 runtime

4. **macOS ARM64** (Apple Silicon)
   - Runner: `macos-latest`
   - Output: `SubdomainScanner-macos-arm64.tar.gz`
   - Self-contained with .NET 9.0 runtime

#### Release Job

After all builds complete successfully:
- Downloads all platform artifacts
- Extracts version from git tag (or generates dev version)
- Creates comprehensive release notes
- Publishes GitHub Release with all platform binaries

### Release Assets

Each release includes:
- Windows executable package (.zip)
- Linux executable package (.tar.gz)
- macOS Intel executable package (.tar.gz)
- macOS Apple Silicon executable package (.tar.gz)

All packages are **self-contained** and include the .NET 9.0 runtime.

### Version Tagging Convention

Use semantic versioning for tags:
- `v1.0.0` - Major release
- `v1.1.0` - Minor release (new features)
- `v1.1.1` - Patch release (bug fixes)
- `v2.0.0-beta.1` - Pre-release

### Manual Release Steps

1. Ensure all changes are committed
2. Create a version tag:
   ```bash
   git tag -a v1.0.0 -m "Release version 1.0.0"
   ```
3. Push the tag:
   ```bash
   git push origin v1.0.0
   ```
4. Wait for the workflow to complete (~5-10 minutes)
5. Check the Releases page for the new release

### Troubleshooting

**Build fails:**
- Check the Actions logs for specific errors
- Ensure all dependencies are correctly referenced in the .csproj
- Verify .NET SDK version compatibility

**Artifacts not uploaded:**
- Check artifact upload step in workflow logs
- Verify artifact paths are correct
- Ensure build output exists in expected location

**Release not created:**
- Verify GitHub token permissions (automatic for repo)
- Check if tag format matches trigger pattern (`v*.*.*`)
- Ensure all build jobs completed successfully

### Local Testing

To test the build locally before triggering the workflow:

```bash
# Windows
dotnet publish SubdomainScanner.Avalonia/SubdomainScanner.Avalonia.csproj -c Release -r win-x64 --self-contained

# Linux
dotnet publish SubdomainScanner.Avalonia/SubdomainScanner.Avalonia.csproj -c Release -r linux-x64 --self-contained

# macOS x64
dotnet publish SubdomainScanner.Avalonia/SubdomainScanner.Avalonia.csproj -c Release -r osx-x64 --self-contained

# macOS ARM64
dotnet publish SubdomainScanner.Avalonia/SubdomainScanner.Avalonia.csproj -c Release -r osx-arm64 --self-contained
```

### Notes

- Self-contained builds are larger (~80-120 MB per platform) but require no dependencies
- Builds run in parallel to minimize total workflow time
- Release is automatically created only when all platform builds succeed
- Pre-release flag is set for tags containing "dev-", "alpha", "beta", or "rc"
