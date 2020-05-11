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
        private string firstLocale = string.Empty;

        public HTMLtoJSONProcessor(HTMLDirectory htmlDirectory, JSONDirectory jsonDirectory, bool verbose)
        {
            this.htmlDirectory = htmlDirectory;
            this.jsonDirectory = jsonDirectory;
            this.verbose = verbose;
        }

        public int Process()
        {
            return ProcessFilesInDirectory(htmlDirectory.GetDirectoryInfo());
        }

        private int ProcessFilesInDirectory(DirectoryInfo dirInfo)
        {
            int errorCount = 0;
            FileInfo[] dirFiles = dirInfo.GetFiles("*.html", SearchOption.TopDirectoryOnly);
            if (dirFiles.Length > 0)
            {
                DirectoryInfo targetDirectory = CreateTargetDirectory(dirInfo);
                foreach (FileInfo file in dirFiles)
                {
                    if (file.FullName.IndexOf("toc.html", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        string jsonFilePath = Path.Combine(targetDirectory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".json");
                        if (this.verbose)
                        {
                            Console.WriteLine("Processing " + file.FullName + " to " + jsonFilePath);
                        }
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.Load(file.FullName);

                        JObject product = new JObject();
                        HtmlNodeCollection nodeMeta = htmlDoc.DocumentNode.SelectNodes("//meta");

                        bool hasTitle = false;
                        bool hasDescription = false;
                        bool hasMSLocale = false;
                        foreach (HtmlNode _htm in nodeMeta)
                        {
                            string tag = string.Empty;
                            string value = string.Empty;
                            HtmlAttributeCollection attribColl = _htm.Attributes;
                            if (attribColl[0].Value == "title")
                            {
                                tag = attribColl[0].Value;
                                value = WebUtility.HtmlDecode(attribColl[1].Value.ToString());
                                product.Add(new JProperty(tag, value));
                                if (!String.IsNullOrWhiteSpace(value))
                                {
                                    hasTitle = true;
                                }
                            }
                            if (attribColl[0].Value == "description")
                            {
                                tag = attribColl[0].Value;
                                value = WebUtility.HtmlDecode(attribColl[1].Value.ToString());
                                product.Add(new JProperty(tag, value));
                                if (!String.IsNullOrWhiteSpace(value))
                                {
                                    hasDescription = true;
                                }
                            }
                            if (attribColl[0].Value == "ms.locale")
                            {
                                if (String.IsNullOrWhiteSpace(firstLocale))
                                {
                                    if (String.IsNullOrWhiteSpace(attribColl[1].Value))
                                    {
                                        Console.WriteLine("ms.locale requires a value in " + file.FullName);
                                    }
                                    else
                                    {
                                        firstLocale = attribColl[1].Value;
                                    }
                                }
                                tag = attribColl[0].Value;
                                value = attribColl[1].Value.ToString();
                                if (value.CompareTo(firstLocale) != 0)
                                {
                                    Console.WriteLine("ms.locale metadata value (" + value + ") in " + file.FullName + " does not match the first ms.locale metadata value found (" + firstLocale + "). All ms.locale metadata values should be identical.");
                                    errorCount++;
                                }
                                product.Add(new JProperty(tag, value));
                                if (!String.IsNullOrWhiteSpace(value))
                                {
                                    hasMSLocale = true;
                                }
                            }
                            if (attribColl[0].Value == "ms.search.form" || attribColl[0].Value == "ms.search.scope" || attribColl[0].Value == "ms.search.region")
                            {
                                tag = attribColl[0].Value;
                                value = attribColl[1].Value.ToString();
                                product.Add(new JProperty(tag, value));
                            }
                        }
                        if (!hasTitle)
                        {
                            Console.WriteLine("title metadata missing in " + file.FullName);
                            errorCount++;
                        }
                        if (!hasDescription)
                        {
                            Console.WriteLine("description metadata missing in " + file.FullName);
                            errorCount++;
                        }
                        if (!hasMSLocale)
                        {
                            Console.WriteLine("ms.locale metadata missing in " + file.FullName);
                            errorCount++;
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
                errorCount = errorCount + (ProcessFilesInDirectory(subdirectoryInfo));
            }
            return errorCount;
        }

        private DirectoryInfo CreateTargetDirectory(DirectoryInfo dirInfo)
        {
            DirectoryInfo targetDirectory = new DirectoryInfo(dirInfo.FullName.Replace(htmlDirectory.GetDirectoryInfo().FullName, jsonDirectory.GetDirectoryInfo().FullName));
            targetDirectory.Create();
            return targetDirectory;
        }
    }
}
