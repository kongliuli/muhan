namespace ModernBoxes.Core.Interfaces
{
    public interface IProcessLauncher
    {
        void Start(string fileName, bool useShellExecute = false);
    }
}
