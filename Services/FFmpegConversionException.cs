using System;

namespace FFgui.Services;

public class FFmpegConversionException : Exception
{
    public FFmpegConversionException(string log) : base(log) { }
}
