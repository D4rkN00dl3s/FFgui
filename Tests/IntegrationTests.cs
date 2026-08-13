using System;
using System.IO;
using System.Threading.Tasks;
using FFgui.Models;
using FFgui.Services;
using FFgui.ViewModels;
using Xunit;

namespace FFgui.Tests;

/// <summary>
/// Exercises real process-spawn paths end-to-end.
/// Both tests are environment-agnostic: a missing tool yields a per-job
/// FFmpegConversionException (still Error + non-empty message), so the suite
/// is green whether or not ffmpeg/ffprobe are installed.
/// </summary>
public class ErrorSurfacingTests
{
    [Fact]
    public async Task StartConversion_surfaces_error_for_non_video_file()
    {
        // non-video input: ffprobe yields no parseable duration
        var tmp = Path.GetTempFileName();
        await File.WriteAllTextAsync(tmp, "not a video");

        var vm = new MainViewModel(new FilePickerService(), new FFmpegService());
        vm.Jobs.Add(new ConversionJobViewModel(new ConversionJob(tmp)));

        await vm.StartConversionCommand.ExecuteAsync(null);

        var job = vm.Jobs[0];
        Assert.Equal(ConversionStatus.Error, job.Status);
        Assert.False(string.IsNullOrEmpty(job.ErrorMessage));

        // only assert the parse-failure branch when ffprobe actually ran
        if (!job.ErrorMessage!.Contains("not found on PATH", StringComparison.OrdinalIgnoreCase))
            Assert.Contains("duration", job.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsToolAvailable_returns_false_for_missing_tool()
    {
        var service = new FFmpegService();
        Assert.False(service.IsToolAvailable("ffmpeg-does-not-exist-xyz"));
    }
}
