namespace ModernBoxes.Sdk.Plugins
{
    public interface IUserNotifier
    {
        void ShowWarning(string title, string message);
        bool ShowConfirm(string title, string message);
    }
}
