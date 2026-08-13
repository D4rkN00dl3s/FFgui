using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FFgui.Models;

namespace FFgui.Services;

public class FFmpegService : IFFmpegService
{
    public async Task ConvertAsync(string input, string output, IProgress<double> progress, CancellationToken token)
    {
        var duration = await GetDurationAsync(input, token);
        var errorLog = new StringBuilder();

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i \"{input}\" -progress pipe:1 -nostats -y \"{output}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null) return;

            if (e.Data.StartsWith("out_time_us="))
            {
                var rest = e.Data.Split('=', 2);
                if (rest.Length == 2 && long.TryParse(rest[1], out var microseconds))
                {
                    var seconds = microseconds / 1_000_000.0;
                    progress.Report(Math.Min(seconds / duration * 100, 100));
                }
            }
            else if (e.Data == "progress=end")
            {
                progress.Report(100);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null) errorLog.AppendLine(e.Data);
        };

        StartProcess(process);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await using var registration = token.Register(() => { try { process.Kill(); } catch { } });
        await process.WaitForExitAsync(token);

        if (process.ExitCode != 0)
        {
            throw new FFmpegConversionException(errorLog.ToString());
        }
    }
    
    private async Task<double> GetDurationAsync(string input, CancellationToken token)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{input}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        StartProcess(process);
        var output = await process.StandardOutput.ReadToEndAsync(token);
        await process.WaitForExitAsync(token);

        if (!double.TryParse(output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var duration) || duration <= 0)
            throw new FFmpegConversionException($"Could not determine duration for \"{input}\". The file may be corrupted or in an unsupported format.");

        return duration;
    }

    private static void StartProcess(Process process)
    {
        try { process.Start(); }
        catch (Win32Exception)
        {
            throw new FFmpegConversionException($"{process.StartInfo.FileName} not found on PATH. Install FFmpeg and add it to your PATH, then restart FFgui.");
        }
    }

    public bool IsToolAvailable(string tool)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = tool,
                Arguments = "--version",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try { StartProcess(process); }
        catch (FFmpegConversionException) { return false; }

        process.WaitForExit();
        return process.ExitCode == 0;
    }

    public static string? GetToolWarningMessage(IEnumerable<string> missing)
    {
        if (missing == null) return null;
        var joined = string.Join(" and ", missing);
        return string.IsNullOrEmpty(joined) ? null : $"{joined} not found on PATH. Install FFmpeg (https://ffmpeg.org) and add it to your PATH, then restart FFgui to enable conversion.";
    }
}