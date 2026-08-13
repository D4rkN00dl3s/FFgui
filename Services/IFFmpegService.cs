using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFgui.Services;

public interface IFFmpegService
{
    Task ConvertAsync(string input, string output, IProgress<double> progress, CancellationToken token);

    bool IsToolAvailable(string tool);
}