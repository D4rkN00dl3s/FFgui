using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FFgui.ViewModels;

namespace FFgui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        QueueDropZone.AddHandler(DragDrop.DragOverEvent, Queue_DragOver);
        QueueDropZone.AddHandler(DragDrop.DropEvent, Queue_Drop);
        QueueDropZone.AddHandler(DragDrop.DragLeaveEvent, Queue_DragLeave);
    }

    private void Queue_DragOver(object? sender, DragEventArgs e)
    {
        var files = e.DataTransfer.TryGetFiles();
        var hasFiles = files is { Length: > 0 };

        QueueDropZone.Classes.Set("dragover", hasFiles);

        e.DragEffects = hasFiles
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private void Queue_Drop(object? sender, DragEventArgs e)
    {
        QueueDropZone.Classes.Set("dragover", false);

        var files = e.DataTransfer.TryGetFiles();

        if (files is not { Length: > 0 })
            return;

        var paths = files
            .OfType<IStorageFile>()
            .Select(file => file.TryGetLocalPath())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .ToList();

        if (paths.Count == 0)
            return;

        if (DataContext is MainViewModel vm)
        {
            vm.AddFiles(paths);
        }

        e.Handled = true;
    }
    
    private void Queue_DragLeave(object? sender, RoutedEventArgs e)
    {
        QueueDropZone.Classes.Set("dragover", false);
        e.Handled = true;
    }
}