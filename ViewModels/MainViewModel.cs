using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FFgui.Models;
using FFgui.Services;

namespace FFgui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string? toolWarningMessage;

    [ObservableProperty]
    private bool isToolWarningVisible;

    public ObservableCollection<ConversionJobViewModel> SelectedJobs { get; } = [];

    private readonly IFilePickerService _filePickerService;
    private readonly IFFmpegService _ffmpegService;

    public MainViewModel(IFilePickerService filePickerService, IFFmpegService ffmpegService)
    {
        _filePickerService = filePickerService;
        _ffmpegService = ffmpegService;
        SelectedJobs.CollectionChanged += (_, _) => RemoveJobCommand.NotifyCanExecuteChanged();

        ProbeToolAvailability();
    }

    /// <summary>
    /// Non-blocking startup probe for ffmpeg/ffprobe on PATH. Updates the banner
    /// properties (which the XAML binds) without freezing the UI. Runs once at startup.
    /// </summary>
    private void ProbeToolAvailability()
    {
        _ = Task.Run(() =>
        {
            try
            {
                var missing = new List<string>();
                if (!_ffmpegService.IsToolAvailable("ffmpeg")) missing.Add("ffmpeg");
                if (!_ffmpegService.IsToolAvailable("ffprobe")) missing.Add("ffprobe");

                var message = FFmpegService.GetToolWarningMessage(missing);
                if (message is null) return;

                Dispatcher.UIThread.Post(() =>
                {
                    ToolWarningMessage = message;
                    IsToolWarningVisible = true;
                });
            }
            catch
            {
                // a failed diagnosis must never block startup
            }
        });
    }

    [RelayCommand]
    private void DismissToolWarning() => IsToolWarningVisible = false;
    
    public ObservableCollection<ConversionJobViewModel> Jobs { get; } = [];

    [RelayCommand]
    private async Task AddFile()
    {
        
        var files = await _filePickerService.PickFilesAsync();

        foreach (var file in files)
        {
            Jobs.Add(new ConversionJobViewModel(new ConversionJob(file)));
        }
        
        OnPropertyChanged(nameof(HasNoJobs));
    }
    
    public void AddFiles(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            Jobs.Add(
                new ConversionJobViewModel(
                    new ConversionJob(file)
                )
            );
        }

        OnPropertyChanged(nameof(HasNoJobs));
    }

    [RelayCommand]
    private async Task StartConversion(CancellationToken token)
    {

        foreach (var job in Jobs)
        {
            job.Status = ConversionStatus.Running;
            try
            {

                var progress = new Progress<double>(value => job.Progress = value);
                await _ffmpegService.ConvertAsync(job.InputFile, job.OutputFile, progress, token);

                job.Status = ConversionStatus.Success;
            }
            catch (OperationCanceledException)
            {
                job.Status = ConversionStatus.Cancelled;
                break;
            }
            catch (FFmpegConversionException ex)
            {
                job.Status = ConversionStatus.Error;
                job.ErrorMessage = ex.Message;
            }

        }
    }

    private bool CanRemoveJobs() => SelectedJobs.Count > 0;
    
    public bool HasNoJobs => Jobs.Count == 0;
    
    [RelayCommand(CanExecute = nameof(CanRemoveJobs))]
    private void RemoveJob()
    {
        foreach (var job in SelectedJobs.ToList())
        {
            Jobs.Remove(job);
        }
        
        OnPropertyChanged(nameof(HasNoJobs));
    }
    
    [RelayCommand]
    private void ClearQueue()
    {
        Jobs.Clear();
        OnPropertyChanged(nameof(HasNoJobs));
    }
    
}