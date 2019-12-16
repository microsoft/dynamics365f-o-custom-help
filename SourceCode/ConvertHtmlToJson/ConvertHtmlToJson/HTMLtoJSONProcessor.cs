using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;


namespace ConvertHtmlToJson
{
    class HTMLtoJSONProcessor
    {
        private readonly HTMLDirectory htmlDirectory;
        private readonly JSONDirectory jsonDirectory;
        private readonly bool verbose;

        public HTMLtoJSONProcessor(HTMLDirectory htmlDirectory, JSONDirectory jsonDirectory, bool verbose)
        {
            this.htmlDirectory = htmlDirectory;
            this.jsonDirectory = jsonDirectory;
            this.verbose = verbose;
        }

        public void Process()
        {
            ProcessFilesInDirectory(htmlDirectory.GetDirectoryInfo());
        }

        private void ProcessFilesInDirectory(DirectoryInfo dirInfo)
        {
            FileInfo[] dirFiles = dirInfo.GetFiles("*.html", SearchOption.TopDirectoryOnly);
            if (dirFiles.Length > 0)
            {
                DirectoryInfo targetDirectory = CreateTargetDirectory(dirInfo);
                foreach (FileInfo file in dirFiles)
                {
                    string jsonFilePath = Path.Combine(targetDirectory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".json");
                    if (this.verbose)
                    {
                        Console.WriteLine("Processing " + file.FullName + " to " + jsonFilePath);
                    }
                    if (file.FullName.IndexOf("toc.html", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        int i = 0;
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.Load(file.FullName);

                        JObject product = new JObject();
                        HtmlNodeCollection nodeMeta = htmlDoc.DocumentNode.SelectNodes("//meta");

                        string tag = string.Empty;
                        string value = string.Empty;
                        foreach (HtmlNode _htm in nodeMeta)
                        {
                            i++;
                            if (i > 2)
                            {
                                HtmlAttributeCollection attribColl = _htm.Attributes;
                                if (attribColl[0].Value == "title" || attribColl[0].Value == "description")
                                {
                                    tag = attribColl[0].Value;
                                    value = WebUtility.HtmlDecode(attribColl[1].Value.ToString());
                                }
                                else
                                {
                                    tag = attribColl[0].Value;
                                    value = attribColl[1].Value.ToString();
                                }
                                product.Add(new JProperty(tag, value));
                            }
                        }

                        HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//body");
                        product.Add(new JProperty("Content", node.InnerText));

                        StreamWriter sysfile = File.CreateText(jsonFilePath);
                        using (JsonTextWriter writer = new JsonTextWriter(sysfile))
                        {
                            product.WriteTo(writer);
                        }
                    }
                }
            }
            foreach (DirectoryInfo subdirectoryInfo in dirInfo.GetDirectories())
            {
                ProcessFilesInDirectory(subdirectoryInfo);
            }
        }

        private DirectoryInfo CreateTargetDirectory(DirectoryInfo dirInfo)
        {
            DirectoryInfo targetDirectory = new DirectoryInfo(dirInfo.FullName.Replace(htmlDirectory.GetDirectoryInfo().FullName, jsonDirectory.GetDirectoryInfo().FullName));
            targetDirectory.Create();
            return targetDirectory;
        }
    }
}
