namespace ModernBoxes.Core.Interfaces;

public interface IUserNotifier
{
    void ShowWarning(string title, string message);

    /// <summary>返回 true 表示用户确认。</summary>
    bool ShowConfirm(string title, string message);
}
