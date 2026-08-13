using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace FFgui.Models;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            ConversionStatus.Running => Brushes.DodgerBlue,
            ConversionStatus.Success => Brushes.SeaGreen,
            ConversionStatus.Error => Brushes.Crimson,
            ConversionStatus.Cancelled => Brushes.Gray,
            _ => Brushes.SlateGray
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}