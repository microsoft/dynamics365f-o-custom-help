using System.Collections.Immutable;

namespace MainProcessor
{
    public interface ILinkProcessor
    {
        string GetLogContent(LogType logType);
        ImmutableArray<string> GetFilesToRemove();
        ImmutableArray<string> GetCopiedFiles();
        bool ProcessContentLinks();
    }
}
