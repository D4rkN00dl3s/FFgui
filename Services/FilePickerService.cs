using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace FFgui.Services;

public class FilePickerService : IFilePickerService
{
    public async Task<IReadOnlyList<string>> PickFilesAsync()
    {
        var topLevel = GetTopLevel();
        if (topLevel is null) return [];

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select video files",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("Video files")
                {
                    Patterns =
                    [
                        "*.mp4",
                        "*.mkv",
                        "*.avi",
                        "*.mov",
                        "*.webm"
                    ]
                }
            ]
        });

        return files.Select(f => f.Path.LocalPath).ToList();
    }

    private static TopLevel? GetTopLevel()
    {
        return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;
    }
}