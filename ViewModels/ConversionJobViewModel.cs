using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using FFgui.Models;

namespace FFgui.ViewModels;

public partial class ConversionJobViewModel : ObservableObject
{
    private readonly ConversionJob _job;
    
    public string InputFile => _job.InputFile;
    
    public string FileName => Path.GetFileName(_job.InputFile);
    
    public string OutputFile => _job.OutputFile;

    [ObservableProperty] private double progress;
    
    [ObservableProperty] private ConversionStatus status = ConversionStatus.Waiting;
    
    [ObservableProperty] private string? errorMessage;
    
    public ConversionJobViewModel(ConversionJob job)
    {
        _job = job;
    }
}