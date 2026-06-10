# Development Guide — Brian's Claude for Visual Studio

This document records the modernization work done in June 2026 to get the solution building
cleanly on .NET 10 against the Visual Studio 2026 SDK, and explains how to configure, build,
and debug the extension.

---

## Current state

- **Target framework:** `net10.0-windows10.0.26100.0` (all three projects)
- **VS SDK:** `Microsoft.VisualStudio.SDK` 18.2.38048 (VS 2026 line)
- **Build:** 0 errors / 0 warnings via `dotnet build`
- **Tests:** 12/12 passing (`dotnet test`)
- **Output:** `BriansClaudeVS.Extension.vsix` produced under
  `src/BriansClaudeVS.Extension/bin/Debug/net10.0-windows10.0.26100.0/`

## Prerequisites

- **Visual Studio 2026** (18.x — currently installed at `C:\Program Files\Microsoft Visual Studio\18\Insiders`)
- **.NET 10 SDK** (10.0.300 or later)
- An **Anthropic API key** for runtime use (not needed to build)

## Solution layout

| Project | Purpose |
|---|---|
| `BriansClaudeVS.Core` | API client (Anthropic SDK), DPAPI credential store, slash-command parsing. No VS dependencies — unit-testable. |
| `BriansClaudeVS.Extension` | The VSIX: package, chat tool window (WPF), options page, commands. |
| `BriansClaudeVS.Tests` | xUnit tests for Core. |

---

## Work completed (June 2026 session)

### 1. .NET 10 compile fixes

The net472 → net10 retarget had been left incomplete. Fixed:

- Garbled/duplicated `GetService` line in `ServiceLocator.cs` (masked every other error).
- `IVsWritableSettingsStore` calls rewritten for COM-style `int` HRESULT returns
  (`CollectionExists`/`GetString` return HRESULTs with `out` flags, not `bool`).
- `ClaudeOptionsPage` now derives from `UIElementDialogPage` (the WPF options-page base
  that exposes `Child`), not `DialogPage`.
- `ProvideOptionPage` attribute switched to positional arguments.
- Extension project gained `ImplicitUsings`, `UseWPF` (XAML codegen), and `UseWindowsForms`
  (needed only for the `System.Design` reference behind `OleMenuCommandService.AddCommand`);
  the implicit `System.Windows.Forms` using is removed to avoid WPF type collisions.
- Analyzer fixes: `CancelAsync()` instead of `Cancel()` (VSTHRD103), removed
  `reader.EndOfStream` in an async loop (CA2024), nullable event-handler `sender` (CS8622).

### 2. Dependency cleanup

- Removed unused `MessagePack` and `System.Drawing.Common` references.
- `System.Security.Cryptography.ProtectedData` / `Microsoft.Extensions.Http`:
  .NET 11 previews → stable 10.0.9.
- `xunit.runner.visualstudio` off prerelease to 3.1.5; `xunit`, `Moq`,
  `Polly.Extensions.Http` wildcards pinned.
- `Microsoft.VSSDK.BuildTools` → 18.6.38345.

### 3. Visual Studio 2026 SDK

- `Microsoft.VisualStudio.SDK` 17.14 → **18.2.38048**, the latest *stable* 18.x
  (18.3–18.9 are preview-only as of June 2026).
- The 18.x SDK is **not on nuget.org** — `NuGet.config` at the repo root adds Microsoft's
  public `vssdk` feed (`https://pkgs.dev.azure.com/azure-public/vside/_packaging/vssdk/nuget/v3/index.json`).
- `OpenTelemetry.Api` 1.15.3 pinned directly to override the SDK's vulnerable transitive
  1.12.0 (NU1902, GHSA-g94r-2vxg-569j).
- VSIX manifest installation floor raised to `[18.0,)` with explicit `ProductArchitecture`
  (amd64 + arm64) per target.

### 4. Windows TFM revision gotcha

`net10.0-windows10.0.26100.1` (revision **1**) is the SDK's opt-in to the **CsWinRT 3.0
preview** generator, which crashes (`MSB6011`). Revision **0** (`...26100.0`) targets the
same Windows 11 24H2 API surface on stable CsWinRT 2.x. Keep the revision at `.0`.

Note: the TFM platform version sets the compile-time API surface. The *runtime* minimum OS
is `SupportedOSPlatformVersion` (Core currently declares `10.0.17763.0` = Win10 1809+).

### 5. VSIX build & deployment pipeline (why F5 didn't work)

Four independent breaks, all fixed:

1. **`VSSDKBuildToolsAutoSetup=true`** added to the Extension csproj. Without it, the
   BuildTools NuGet package never imports `Microsoft.VSSDK.targets` and the build produces
   a plain DLL — no VSCT compile, no VSIX, no deployment.
2. **`VSPackage.vsct`** changed from a `None` item to **`VSCTCompile`** with
   `ResourceName=Menus.ctmenu` (matches `ProvideMenuResource`).
3. **Icon assets** (`Resources/ClaudeIcon.png`, `assets/preview.png`) did not exist;
   placeholder PNGs were generated and are included in the VSIX as `Content`.
   *(Replace with real artwork before publishing.)*
4. **Pkgdef is hand-authored.** `CreatePkgDef.exe` is a .NET Framework tool and cannot
   reflect over a .NET 10 assembly, so `GeneratePkgDefFile=false` and registration ships as
   `src/BriansClaudeVS.Extension/BriansClaudeVS.Extension.pkgdef`.

   > ⚠️ **If you add, remove, or change any registration attribute on
   > `BriansClaudeVSPackage` (or change a GUID), you must update the .pkgdef by hand.**
   > A mismatch shows up as a silent package-load failure (see Troubleshooting).

Deployment is wired as:

- csproj: `DeployExtension=true` only when `Configuration=Debug` **and**
  `BuildingInsideVisualStudio=true`. CLI `dotnet build` deliberately does not deploy
  (the VSSDK targets hard-error on deployment under `dotnet build`) but still produces the VSIX.
- sln: the Extension project has a `Deploy.0` entry for Debug — this is the **Deploy**
  checkbox in *Build → Configuration Manager*. If deployment silently stops working,
  check that box first; VS rewrites the .sln and can drop the line.

VSIX manifest notes (schema is strict):

- `Metadata` children are order-sensitive: `Identity, DisplayName, Description,
  GettingStartedGuide, Icon, PreviewImage, Tags`.
- Every `InstallationTarget` requires a `ProductArchitecture` child.
- The `VsPackage` asset points directly at `BriansClaudeVS.Extension.pkgdef`
  (not the `PkgdefProjectOutputGroup`, which is empty with generation disabled).

---

## Configuration

### Building

```powershell
dotnet build          # full solution, produces the VSIX, does NOT deploy
dotnet test           # runs the 12 Core unit tests
```

First restore needs internet access to both nuget.org and the public `vssdk` feed
(no authentication required; `NuGet.config` handles it).

### Runtime configuration (inside VS)

1. **Tools → Options → Brian's Claude → General**
2. Paste your Anthropic API key (stored encrypted via DPAPI, never in plain settings),
   pick chat/inline models, and use **Verify** to test the key with a live API call.

---

## Debugging the extension

### Normal F5 loop

1. Open `BriansClaudeForVS.sln` in VS 2026.
2. Set **BriansClaudeVS.Extension** as the startup project.
3. **F5.** The csproj launch settings (`StartProgram=$(DevEnvDir)devenv.exe`,
   `StartArguments=/rootsuffix Exp`) start the **experimental instance** with the
   freshly deployed VSIX and the debugger attached.
4. In the experimental instance, **open any solution or project** — the package autoloads
   on `SolutionExists` (background). Breakpoints in `BriansClaudeVSPackage.InitializeAsync`
   won't hit until then.
5. Invoke the chat window command (View → Other Windows). `OpenChatWindowCommand.Execute`
   is a good first breakpoint.

`$(DevEnvDir)` only exists inside VS, so this launch profile cannot run from the CLI.
For a manual loop: `dotnet build`, then install with
`VSIXInstaller.exe /rootSuffix:Exp <path-to-vsix>` and start
`devenv.exe /rootsuffix Exp` yourself.

### Troubleshooting

| Symptom | Fix |
|---|---|
| "Extension '…' could not be found. Please make sure the extension has been installed." | The Exp hive's extension cache is stale. Run `devenv /rootsuffix Exp /updateconfiguration`, then F5 again. |
| "Deploy is not enabled" | Tick **Deploy** for the Extension project in *Build → Configuration Manager* (Debug). Verify the csproj `DeployExtension` condition still evaluates true inside VS. |
| Hive is corrupted / weird stale behavior | Close VS, delete `%LOCALAPPDATA%\Microsoft\VisualStudio\18.0_*Exp` **and** `%APPDATA%\Microsoft\VisualStudio\18.0_*Exp`, F5 to recreate fresh. |
| Package never loads, no error shown | Launch with logging: `devenv /rootsuffix Exp /log`, then read `%APPDATA%\Microsoft\VisualStudio\18.0_*Exp\ActivityLog.xml`. Hand-pkgdef typos and MEF failures land here. |
| Breakpoints "symbols not loaded" | Stale deployment: clean, delete the extension folder under `%LOCALAPPDATA%\...\18.0_*Exp\Extensions\BrianProgrammer`, rebuild. |
| Exceptions swallowed silently | Debug → Windows → Exception Settings → enable break on thrown CLR exceptions. |
| `MSB6011` in `RunCsWinRTGenerator` after editing the TFM | The Windows TFM revision was set to `.1`. Use `.0` (see §4 above). |

### Known open items

- **Runtime validation pending:** the build/deploy chain is verified, but the extension has
  not yet been confirmed loading end-to-end in the experimental instance.
- Placeholder icons need real artwork.
- The 18.x SDK still only ships .NET Framework dependency groups, so `NU1701` stays
  suppressed in the Extension csproj (along with `WFO1000`, a WinForms designer analyzer
  that doesn't apply to `DialogPage` types).
- PR for all of this work: https://github.com/BrianMinister/BriansClaudeForVS/pull/2
