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

- [ ] Add CI workflow (restore → build → test)
  - Area: .github/workflows/
  - Type: infra
  - Rationale: no CI; prevents broken builds reaching humans.
  - Dependencies: test project exists (do After)
  - Success criteria: workflow runs on push/PR and is green on a clean tree.

## Later

- [ ] Surface ffmpeg/ffprobe-missing as a startup banner (not only per-job at first Start)
  - Area: App.axaml.cs / Services
  - Type: feature
  - Rationale: current first-use error is good, but a startup probe gives clearer upfront feedback when the toolchain is absent.
  - Dependencies: none beyond existing FFmpegService
  - Success criteria: app shows a clear "ffmpeg not found" message before any job runs. `needs clarification` — prefer startup banner vs. per-job only?

- [ ] Decide on Windows-only `app.manifest`
  - Area: app.manifest / FFgui.csproj
  - Type: refactor/infra
  - Rationale: `app.manifest` is Windows-only (ignored on Linux/Mac) and `OutputType=WinExe` is Windows-centric; Avalonia notes it may matter for transparency/embedded controls.
  - Dependencies: none
  - Success criteria: builds on Windows; unchanced on Linux/Mac. `needs clarification` — keep (template) or drop?

- [ ] Normalize `ConversionJobViewModel.ErrorMessage` `[ObservableProperty] public partial` placement
  - Area: ViewModels/ConversionJobViewModel
  - Type: refactor (style)
  - Rationale: inconsistent with the other `[ObservableProperty]` private fields; cosmetic only.
  - Dependencies: none
  - Success criteria: builds; property generation unchanged. `needs clarification` — worth the churn?

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
