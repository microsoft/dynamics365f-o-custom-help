namespace MainProcessor
{
    public interface ILogger
    {
        void LogInfo(string message = null, bool newLine = true);
        void LogWarning(string message = null, bool newLine = true);
        void LogError(string message = null, bool newLine = true);
        string GetLogContent();
    }
}
