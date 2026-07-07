namespace ModernBoxes.Core.Interfaces;

public interface IIconExtractor
{
    string ExtractIconToFile(string appPath, string outputDirectory, string fileName);
}
