using ModernBoxes.Core.Enums;

namespace ModernBoxes.Core.Models
{
    public class DirInformationModel
    {
        public string Path { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Space { get; set; } = string.Empty;
        public string Include { get; set; } = string.Empty;
        public string CreateTime { get; set; } = string.Empty;
        public string DirName { get; set; } = string.Empty;
        public DirEnum DirKind { get; set; }
    }
}
