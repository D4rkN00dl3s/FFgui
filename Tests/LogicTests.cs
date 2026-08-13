using System.Globalization;
using System.IO;
using Avalonia.Media;
using FFgui.Models;
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
