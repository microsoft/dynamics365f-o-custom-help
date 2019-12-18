using System;
using System.IO;
using HtmlFromRepoGenerator;
using Xunit;

namespace HtmlFromRepoGeneratorTests
{
    public class LogFilesTest
    {
        [Fact]
        public void Test_Ctor_Throws_If_Called_With_Illegal_Args()
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => new LogFiles(string.Empty));
            Assert.Contains("path", ex.Message);
        }

        [Fact]
        public void Test_Ctor_Initializes_Log_Paths_If_Absolute_Path_Provided()
        {
            LogFiles logFiles = new LogFiles(@"c:\users\some-user\logs");
            
            const string consoleLog = @"c:\users\some-user\logs\output.txt";
            Assert.Equal(consoleLog, logFiles.ConsoleLog);

            const string removedFilesLog = @"c:\users\some-user\logs\removedFiles.txt";
            Assert.Equal(removedFilesLog, logFiles.RemovedFilesLog);

            const string normalFilesLog = @"c:\users\some-user\logs\normalFiles.txt";
            Assert.Equal(normalFilesLog, logFiles.NormalFilesLog);

            const string notExistentFilesLog = @"c:\users\some-user\logs\notExistentFiles.txt";
            Assert.Equal(notExistentFilesLog, logFiles.NotExistentFilesLog);

            const string copiedFilesLog = @"c:\users\some-user\logs\copiedFiles.txt";
            Assert.Equal(copiedFilesLog, logFiles.CopiedFilesLog);

            const string replacedLinksLog = @"c:\users\some-user\logs\replacedLinks.csv";
            Assert.Equal(replacedLinksLog, logFiles.ReplacedLinksLog);
            
            const string replacedLanguageLinksLog = @"c:\users\some-user\logs\replacedLanguageLinks.csv";
            Assert.Equal(replacedLanguageLinksLog, logFiles.ReplacedLanguageLinksLog);
        }

        [Fact]
        public void Test_Ctor_Initializes_Log_Paths_If_Relative_Path_Provided()
        {
            LogFiles logFiles = new LogFiles("logs");

            const string consoleLog = @"logs\output.txt";
            Assert.Equal(consoleLog, logFiles.ConsoleLog);

            const string removedFilesLog = @"logs\removedFiles.txt";
            Assert.Equal(removedFilesLog, logFiles.RemovedFilesLog);

            const string normalFilesLog = @"logs\normalFiles.txt";
            Assert.Equal(normalFilesLog, logFiles.NormalFilesLog);

            const string notExistentFilesLog = @"logs\notExistentFiles.txt";
            Assert.Equal(notExistentFilesLog, logFiles.NotExistentFilesLog);

            const string copiedFilesLog = @"logs\copiedFiles.txt";
            Assert.Equal(copiedFilesLog, logFiles.CopiedFilesLog);

            const string replacedLinksLog = @"logs\replacedLinks.csv";
            Assert.Equal(replacedLinksLog, logFiles.ReplacedLinksLog);

            const string replacedLanguageLinksLog = @"logs\replacedLanguageLinks.csv";
            Assert.Equal(replacedLanguageLinksLog, logFiles.ReplacedLanguageLinksLog);
        }
    }
}
