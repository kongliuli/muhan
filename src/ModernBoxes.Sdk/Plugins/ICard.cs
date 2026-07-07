namespace ModernBoxes.Sdk.Plugins
{
    public interface ICard
    {
        string CardID { get; set; }
        string CardName { get; set; }
        object CardContent { get; set; }
        double CardHeight { get; set; }
        string Preview { get; set; }
        bool IsChecked { get; set; }
    }
}
