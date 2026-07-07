namespace ModernBoxes.Core.Models
{
    public enum ResultType { Application, File, Directory, Note, Menu }

    public class SearchResultModel
    {
        public ResultType Type { get; set; }
        public string Name { get; set; } = "";
        public string Detail { get; set; } = "";
        public string IconText { get; set; } = "";
        public object? ActionTarget { get; set; }
    }
}
