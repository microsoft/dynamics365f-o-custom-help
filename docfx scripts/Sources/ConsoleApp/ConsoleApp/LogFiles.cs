using System;
using System.IO;

namespace ConsoleApp
{
    internal sealed class LogFiles
    {
        private readonly string _path;
        private string _consoleLog;
        private string _removedFilesLog;
        private string _normalFilesLog;
        private string _notExistentFilesLog;
        private string _copiedFilesLog;
        private string _replacedLinksLog;
        private string _replacedLanguageLinksLog;

        internal LogFiles(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            _path = path;
        }

        public string ConsoleLog
        {
            get
            {
                if (string.IsNullOrEmpty(_consoleLog))
                    _consoleLog = Path.Combine(_path, "output.txt");
                return _consoleLog;
            }
        }

        public string RemovedFilesLog
        {
            get
            {
                if (string.IsNullOrEmpty(_removedFilesLog))
                    _removedFilesLog = Path.Combine(_path, "removedFiles.txt");
                return _removedFilesLog;
            }
        }

        public string NormalFilesLog
        {
            get
            {
                if (string.IsNullOrEmpty(_normalFilesLog))
                    _normalFilesLog = Path.Combine(_path, "normalFiles.txt");
                return _normalFilesLog;
            }
        }

        public string NotExistentFilesLog
        {
            get
            {
                if (string.IsNullOrEmpty(_notExistentFilesLog))
                    _notExistentFilesLog = Path.Combine(_path, "notExistentFiles.txt");
                return _notExistentFilesLog;
            }
        }

        public string CopiedFilesLog
        {
            get
            {
                if (string.IsNullOrEmpty(_copiedFilesLog))
                    _copiedFilesLog = Path.Combine(_path, "copiedFiles.txt");
                return _copiedFilesLog;
            }
        }

        public string ReplacedLinksLog
        {
            get
            {
                if (string.IsNullOrEmpty(_replacedLinksLog))
                    _replacedLinksLog = Path.Combine(_path, "replacedLinks.csv");
                return _replacedLinksLog;
            }
        }

        public string ReplacedLanguageLinksLog
        {
            get
            {
                if (string.IsNullOrEmpty(_replacedLanguageLinksLog))
                    _replacedLanguageLinksLog = Path.Combine(_path, "replacedLanguageLinks.csv");
                return _replacedLanguageLinksLog;
            }
        }
    }
}
