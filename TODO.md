# Project TODO / Tickets

FFgui is a working but spare .NET 10/Avalonia frontend for FFmpeg. Near-term focus is stability (ffmpeg/ffprobe are hard-required at runtime with only a first-use error check), test coverage (none exists), and removing leftover demo/dead code so the codebase is safe to refactor.

## Now

- [x] Add `.gitignore` for bin/obj + Rider `.idea/`
  - Area: repo root
  - Type: infra
  - Rationale: `bin`, `obj`, and `.idea/` are already present in the tree; sharing or initializing VCS without ignoring them causes churn and accidental commits.
  - Dependencies: none
  - Success criteria: `.gitignore` exists; `dotnet build` reproducibly leaves only expected outputs.

- [x] Add test project + tests for pure logic (ConversionJob naming/collision-suffixing, StatusToBrushConverter)
  - Area: Tests/ (new project)
  - Type: test
  - Rationale: zero coverage blocks safe refactoring of `ConversionJob.GenerateOutputFile` and the status→brush mapping; both are pure and testable without UI/DI.
  - Dependencies: none (pure-model tests need no DI)
  - Success criteria: `dotnet test` runs and passes; a deliberate regression in collision-suffixing is caught.

- [x] Fix startup banner appearing when ffmpeg/ffprobe ARE present
  - Area: Services/FFmpegService, Tests/IntegrationTests.cs
  - Type: bugfix
  - Rationale: `IsToolAvailable` probed tools with `--version`, which ffmpeg/ffprobe reject (exit 8/1), so `exit != 0` was treated as "missing" even with a working toolchain — banner fired despite the "no banner when both tools present" success criterion in the banner ticket below.
  - Dependencies: ffmpeg/ffprobe on PATH (available on this machine)
  - Success criteria: `ffmpeg -version`/`ffprobe -version` exit 0; regression tests assert `IsToolAvailable` returns `true` for the real tools (skipped when absent) and `false` for a bogus name; `dotnet build`/`dotnet test` clean. Changed probe flag from `--version` to `-version`.

- [x] Harden cross-platform build (Windows + Linux)
  - Area: FFgui.csproj, Tests/FFgui.Tests.csproj, FFgui.sln, .github/workflows/ci.yml
  - Type: refactor/infra
  - Rationale: MSBuild path separators in item specs were backslash (`Assets\**`, `Tests\**\*.cs`, `..\FFgui.csproj`, `.sln` project path), the cross-platform-unsafe form; normalized to forward slashes (MSBuild accepts both on Windows, forward-slash everywhere is the documented safe form). CI extended to a `windows-latest` × `ubuntu-latest` matrix so the build+tests run on both.
  - Dependencies: none
  - Success criteria: `dotnet build`/`dotnet test` green on both runners; ubuntu-latest has ffmpeg preinstalled so the positive `IsToolAvailable` regression tests execute there (Windows skips them gracefully when ffmpeg is absent). No local Linux host was available in this environment (no WSL/Docker), so Linux is validated via CI only.

- [x] Verify error-surfacing path end-to-end with a corrupt/non-video input
  - Area: Views/MainWindow + ViewModels/MainViewModel (UI)
  - Type: bugfix/verify
  - Rationale: the ffprobe-parse and missing-tool crashes were fixed compile-only; confirm a non-video file and a missing-ffmpeg scenario both render a per-job error in the UI rather than crashing.
  - Dependencies: ffmpeg/ffprobe on PATH (available on this machine)
  - Success criteria: dropping a non-video file into the queue and clicking Start shows its error in the Expander; no unhandled crash.

## Next

- [x] Remove demo `Greeting`/`ChangeGreet` from MainViewModel
  - Area: ViewModels/MainViewModel
  - Type: refactor (dead code)
  - Rationale: `Greeting`/`ChangeGreet` are defined but never bound in `MainWindow.axaml` (header text is hardcoded); leftover template/demo code.
  - Dependencies: none
  - Success criteria: build 0 warnings/0 errors; no references to `Greeting`/`ChangeGreet` remain.

- [x] Move `FFmpegConversionException` to its own file
  - Area: Services/
  - Type: refactor (hygiene)
  - Rationale: exception class is co-located in `FFmpegService.cs`; one-type-per-file matches the rest of the codebase.
  - Dependencies: none
  - Success criteria: builds clean; all references resolve unchanged.

- [x] Add CI workflow (restore → build → test)
  - Area: .github/workflows/
  - Type: infra
  - Status: file created (`.github/workflows/ci.yml`); runs on push/PR to `main`. **Unverified on GitHub** — not yet committed/pushed by me (git owner's call); locally `dotnet restore && dotnet build && dotnet test` is 0 warnings/0 errors + 16 passed.

## Later

- [x] Surface ffmpeg/ffprobe-missing as a non-blocking, dismissible startup banner
  - Area: ViewModels/MainViewModel + Views/MainWindow + Services/FFmpegService
  - Type: feature
  - Rationale: upfront warning when the toolchain is absent at startup; the per-job Expander remains the conversion-time fallback.
  - Dependencies: none
  - Success criteria: banner names the missing tool(s) at startup; dismissible without re-probe; no banner when both tools present. Implemented as a non-blocking startup banner per the accepted design decision.

- [x] Decide on Windows-only `app.manifest`
  - Area: app.manifest / FFgui.csproj
  - Type: refactor/infra
  - Rationale: `app.manifest` is Windows-only (ignored on Linux/Mac) and `OutputType=WinExe` is Windows-centric; Avalonia notes it may matter for transparency/embedded controls.
  - Decision: KEEP. The manifest only declares Windows-10 `<supportedOS>` + the standard DPI/embedded-controls advisory comment; no `trustInfo`/`requestedExecutionLevel` (no `requireAdministrator`) or other unusual settings, so it cannot change Linux/macOS behavior. Dropping it risks Windows DPI regressions for zero offsetting benefit.
  - Dependencies: none
  - Success criteria: builds on Windows; unchanged behavior on Linux/Mac (manifest inert).

- [x] Normalize `ConversionJobViewModel.ErrorMessage` `[ObservableProperty] public partial` placement
  - Area: ViewModels/ConversionJobViewModel
  - Type: refactor (style)
  - Rationale: inconsistent with the other `[ObservableProperty]` private fields; cosmetic only.
  - Dependencies: none
  - Success criteria: builds (0 warnings/0 errors); generated public `string? ErrorMessage` property and INPC behavior unchanged; all 16 tests still pass. Converted `[ObservableProperty] public partial string? ErrorMessage { get; set; }` → `[ObservableProperty] private string? errorMessage;` (callers unaffected — they bind/use the generated `ErrorMessage` property).

- [ ] Replace manual `SelectedJobs` ObservableCollection + `OnPropertyChanged(HasNoJobs)` with reactive bindings
  - Area: ViewModels/MainViewModel
  - Type: refactor
  - Rationale: current pattern works but is verbose and error-prone for future derived state.
  - Dependencies: none
  - Success criteria: selection-based Remove still enables/disables correctly. `needs clarification` — current impl is fine; only worth it if more derived state is added.

## Notes and assumptions

- No git repo initialized in this tree, so no `.git`; `git log`/`git status` unavailable. Ordering inferred from code inspection + the project context file (`CLAUDE.md`).
- Runtime validation done via `dotnet build` (currently 0 warnings, 0 errors). ffmpeg/ffprobe confirmed present on PATH on this machine (`/c/Users/Martin/ffmpeg/bin`).
- Product/UX priorities (e.g., feature scope) are unknown — only technical-risk items are listed; items needing product input are marked `needs clarification`.
