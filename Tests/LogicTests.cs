using System.Globalization;
using System.IO;
using Avalonia.Media;
using FFgui.Models;
using FFgui.Services;
using Xunit;

namespace FFgui.Tests;

public class LogicTests
{
    // --- ConversionJob.OutputFile (pure-ish: touches File.Exists for collision) ---

    [Fact]
    public void OutputFile_uses_converted_suffix_in_same_directory()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var input = Path.Combine(dir, "clip.mov");

        var job = new ConversionJob(input);

        Assert.Equal(input, job.InputFile);
        Assert.Equal(Path.Combine(dir, "clip_converted.mp4"), job.OutputFile);
    }

    [Fact]
    public void OutputFile_suffixes_once_on_collision()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var input = Path.Combine(dir, "clip.mov");
        File.WriteAllText(Path.Combine(dir, "clip_converted.mp4"), "x");

        var job = new ConversionJob(input);

        Assert.Equal(Path.Combine(dir, "clip_converted (1).mp4"), job.OutputFile);
    }

    [Fact]
    public void OutputFile_suffixes_again_when_still_collisioned()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var input = Path.Combine(dir, "clip.mov");
        File.WriteAllText(Path.Combine(dir, "clip_converted.mp4"), "x");
        File.WriteAllText(Path.Combine(dir, "clip_converted (1).mp4"), "x");

        var job = new ConversionJob(input);

        Assert.Equal(Path.Combine(dir, "clip_converted (2).mp4"), job.OutputFile);
    }

    // --- StatusToBrushConverter (pure mapping) ---

    private readonly StatusToBrushConverter _converter = new();

    [Theory]
    [InlineData(ConversionStatus.Running, 0xFF1E90FFu)]      // DodgerBlue
    [InlineData(ConversionStatus.Success, 0xFF2E8B57u)]    // SeaGreen
    [InlineData(ConversionStatus.Error, 0xFFDC143C)]        // Crimson
    [InlineData(ConversionStatus.Cancelled, 0xFF808080u)]    // Gray
    [InlineData(ConversionStatus.Waiting, 0xFF708090u)]     // SlateGray
    public void StatusToBrushConverter_maps_status_to_expected_color(ConversionStatus status, uint argb)
    {
        var brush = Assert.IsAssignableFrom<ISolidColorBrush>(_converter.Convert(status, null!, null, CultureInfo.InvariantCulture));
        Assert.Equal(Color.FromUInt32(argb), brush.Color);
    }
}

public class ToolWarningMessageTests
{
    [Fact]
    public void GetToolWarningMessage_returns_null_when_no_tools_missing()
    {
        Assert.Null(FFmpegService.GetToolWarningMessage(Array.Empty<string>()));
        Assert.Null(FFmpegService.GetToolWarningMessage(new string[] { }));
    }

    [Fact]
    public void GetToolWarningMessage_names_ffmpeg_when_only_ffmpeg_missing()
    {
        var msg = FFmpegService.GetToolWarningMessage(new[] { "ffmpeg" })!;
        Assert.Contains("ffmpeg not found on PATH", msg);
        Assert.DoesNotContain("ffprobe", msg);
    }

    [Fact]
    public void GetToolWarningMessage_names_ffprobe_when_only_ffprobe_missing()
    {
        var msg = FFmpegService.GetToolWarningMessage(new[] { "ffprobe" })!;
        Assert.Contains("ffprobe not found on PATH", msg);
        Assert.DoesNotContain("ffmpeg not found", msg);
    }

    [Fact]
    public void GetToolWarningMessage_names_both_when_missing()
    {
        var msg = FFmpegService.GetToolWarningMessage(new[] { "ffmpeg", "ffprobe" })!;
        Assert.Contains("ffmpeg and ffprobe not found on PATH", msg);
    }
}
