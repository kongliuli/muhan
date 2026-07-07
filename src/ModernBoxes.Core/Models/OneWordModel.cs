namespace ModernBoxes.Core.Models
{
    public class OneWordModel
    {
        public int id { get; set; }
        public string uuid { get; set; } = string.Empty;
        public string hitokoto { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string from { get; set; } = string.Empty;
        public object? from_who { get; set; }
        public string creator { get; set; } = string.Empty;
        public int creator_uid { get; set; }
        public int reviewer { get; set; }
        public string commit_from { get; set; } = string.Empty;
        public string created_at { get; set; } = string.Empty;
        public int length { get; set; }
    }
}
