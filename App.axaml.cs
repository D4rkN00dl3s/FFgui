using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FFgui.Services;
using FFgui.ViewModels;
using FFgui.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FFgui;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        
        collection.AddSingleton<IFilePickerService, FilePickerService>();
        collection.AddSingleton<IFFmpegService, FFmpegService>();
        collection.AddTransient<MainViewModel>();
        
        Services = collection.BuildServiceProvider();
        var vm = Services.GetRequiredService<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {

            desktop.MainWindow = new MainWindow{DataContext = vm};
        }

        base.OnFrameworkInitializationCompleted();
    }
}