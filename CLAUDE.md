# FFgui

## Summary
FFgui is a C#/.NET 10 desktop GUI frontend for FFmpeg. Users add video files to a conversion queue (via file picker or drag-and-drop) and convert them to MP4, with per-job progress tracking, cancel, and per-job error display. Single-window app; jobs convert sequentially.

## Tech stack
- **Language:** C# 10, nullable reference types enabled (`<Nullable>enable</Nullable>`)
- **Framework:** .NET 10 (`net10.0`), `OutputType=WinExe` (no console window)
- **UI:** Avalonia 12.1 (cross-platform XAML), `FluentTheme`, Inter font
- **MVVM:** CommunityToolkit.Mvvm 8.4.2 (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`, `AsyncRelayCommand`)
- **DI:** Microsoft.Extensions.DependencyInjection 10.0.10 (configured in `App.axaml.cs`)
- **External runtime dependency:** `ffmpeg` and `ffprobe` must be on `PATH`
- **Build tooling:** `dotnet` CLI (SDK 8.0 / 10.0 present)
- No code-style analyzer, no `.editorconfig`, no `.ruleset`, no formatter config

## Commands
- Install deps: `dotnet restore`
- Build (debug): `dotnet build`
- Build (release): `dotnet build -c Release`
- Run: `dotnet run`
- Tests: `dotnet test` (xUnit; project `Tests/FFgui.Tests.csproj`, references `FFgui.csproj`; 9 tests — `ConversionJob` collision-suffixing + `StatusToBrushConverter` mapping + an end-to-end error-surfacing integration test through `MainViewModel.StartConversion` against real ffprobe)

## Directory structure
- `Assets/` — app icon (`avalonia-logo.ico`), bundled as `AvaloniaResource`
- `Models/` — domain models: `ConversionJob` (input/output path generation, collision-safe suffixing), `ConversionStatus` (flat status enum), `StatusToBrushConverter` (IValueConverter for status→color)
- `Services/` — `FFmpegService`/`IFFmpegService` (spawns ffmpeg/ffprobe, parses `-progress pipe:1` for progress), `FFmpegConversionException` (in its own file), `FilePickerService`/`IFilePickerService` (Avalonia `StorageProvider` file picker, video filter)
- `ViewModels/` — `MainViewModel`, `ConversionJobViewModel` (per-job wrapper); both inherit `ObservableObject` directly (`ViewModelBase` was removed)
- `Views/` — `MainWindow.axaml` + `MainWindow.axaml.cs` (drag-over/drop/drag-leave handlers)
- `ViewLocator.cs` — standard Avalonia `ITemplate` mapping `ViewModel`→`View` by type-name reflection
- `App.axaml` / `App.axaml.cs` — XAML boot + DI registration (singleton services, transient `MainViewModel`)
- `Program.cs` — `Main` entry → `AppBuilder.Configure<App>()...StartWithClassicDesktopLifetime`

## Conventions
- MVVM via `CommunityToolkit.Mvvm`: bindable props via `[ObservableProperty]`; commands via `[RelayCommand]`. Async commands expose `.IsRunning`, `.Cancel`, `.IsCancellationRequested` on the generated `AsyncRelayCommand`.
- DI: services `AddSingleton`, `MainViewModel` `AddTransient`. Resolve `MainViewModel` from `App.Services` and set as `MainWindow.DataContext` in code (not XAML).
- Nullable enabled; methods return `async Task`/`async Task<T>`.
- Naming: PascalCase, 4-space indent, `FFgui.*` namespace.
- Commands raise `CanExecute` via generated `NotifyCanExecuteChanged` (e.g. `RemoveJobCommand` tied to `CanRemoveJobs`).
- Two "add files" entry points: `AddFileCommand` (file-picker button) and public `MainViewModel.AddFiles(IEnumerable<string>)` (drag-drop path in `MainWindow.axaml.cs`). Both call `OnPropertyChanged(nameof(HasNoJobs))`.
- `ObservableCollection<ConversionJobViewModel> SelectedJobs` bound via `ListBox SelectedItems` (two-way by convention).

## Gotchas / things to watch
- **ffmpeg/ffprobe on PATH required.** `FFmpegService` spawns `ffmpeg`/`ffprobe` by bare name (no search-path fallback). Missing tools and parse failures are now caught and surfaced per-job as `FFmpegConversionException` (rendered via the job's error Expander) rather than crashing; there is no upfront startup banner.
- `GetDurationAsync` now uses `double.TryParse(..., NumberStyles.Float, ...)` with a `duration <= 0` guard; empty/`N/A`/non-parseable ffprobe output (e.g. corrupt or unsupported file) is surfaced per-job as `FFmpegConversionException` rather than throwing.
- `ConversionJobViewModel.ErrorMessage` is declared with `[ObservableProperty] public partial string? ErrorMessage` (unusual `public` partial placement); the property is generated correctly but the style is inconsistent with the other `[ObservableProperty]` fields (which are private).
- `StartConversion` runs jobs sequentially; a single `CancellationToken` cancels the whole queue (the Cancel button binds `StartConversionCommand.Cancel`).
- `MainWindow.axaml.cs` references `QueueDropZone` (set via `x:Name`) — keep this name if restructuring the drop zone.
- `app.manifest` is Windows-only (WinExe + Windows compatibility block); app is built for desktop (`UsePlatformDetect`) — cross-platform (Linux/Mac) build will work but `app.manifest` is ignored outside Windows.
- `.gitignore` now ignores `bin/`, `obj/`, and `.idea/`; add a VCS remote when ready to share.
