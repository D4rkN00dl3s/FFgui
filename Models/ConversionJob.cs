using System.IO;

namespace FFgui.Models;

public class ConversionJob
{
    public string InputFile { get; set; }
    
    public string OutputFile { get; set; }
    
    public ConversionJob(string inputFile)
    {
        InputFile = inputFile;
        OutputFile = GenerateOutputFile(inputFile);
    }
    
    private static string GenerateOutputFile(string inputFile)
    {
        var directory = Path.GetDirectoryName(inputFile) ?? "";
        var name = Path.GetFileNameWithoutExtension(inputFile);
        var candidate = Path.Combine(directory, $"{name}_converted.mp4");

        var counter = 1;
        while (File.Exists(candidate))
        {
            candidate = Path.Combine(directory, $"{name}_converted ({counter}).mp4");
            counter++;
        }

        return candidate;
    }
}