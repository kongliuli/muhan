using ModernBoxes.Core.Interfaces;
using System.IO;

namespace ModernBoxes.Infrastructure.Platform;

public class IconExtractor : IIconExtractor
{
    public string ExtractIconToFile(string appPath, string outputDirectory, string fileName)
    {
        GetIcon.getFileIcon(appPath, outputDirectory, fileName);
        return Path.Combine(outputDirectory, fileName);
    }
}
