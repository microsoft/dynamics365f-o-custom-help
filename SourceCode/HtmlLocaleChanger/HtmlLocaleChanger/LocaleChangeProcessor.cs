using HtmlAgilityPack;
using System;
using System.IO;
using System.Text;

namespace HtmlLocaleChanger
{
    class LocaleChangeProcessor
    {
        private readonly HTMLDirectory htmlDirectory;
        private readonly string requestedLocale;
        private readonly bool verbose;

        public LocaleChangeProcessor(HTMLDirectory htmlDirectory, string requestedLocale, bool verbose)
        {
            this.htmlDirectory = htmlDirectory;
            this.requestedLocale = requestedLocale;
            this.verbose = verbose;
        }

        public void Process()
        {
            ProcessAllHTMLFiles(htmlDirectory.GetDirectoryInfo());
        }

        private void ProcessAllHTMLFiles(DirectoryInfo dirHTML)
        {
            FileInfo[] dirFiles = dirHTML.GetFiles("*.html", SearchOption.AllDirectories);
            if (dirFiles.Length > 0)
            {
                if (verbose)
                {
                    Console.WriteLine(dirFiles.Length + " files to process.");
                }

                foreach (FileInfo file in dirFiles)
                {
                    if (file.Name.ToLower() != "toc.html")
                    {
                        if (verbose)
                        {
                            Console.WriteLine("Processing " + file.FullName);
                        }

                        try
                        {
                            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                            htmlDoc.Load(file.FullName, Encoding.UTF8);
                            HtmlNodeCollection nodeMeta = htmlDoc.DocumentNode.SelectNodes("//meta");
                            foreach (HtmlNode _htm in nodeMeta)
                            {
                                HtmlAttributeCollection attribColl = _htm.Attributes;
                                if (attribColl[0].Value == "ms.locale")
                                {
                                    attribColl[1].Value = requestedLocale;
                                }
                            }
                            htmlDoc.Save(file.FullName, Encoding.UTF8);
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine("Error found while processing " + file.FullName + ": " + ex.Message);
                            Environment.Exit(-1);
                        }
                    }
                }
            }
        }
    }
}
