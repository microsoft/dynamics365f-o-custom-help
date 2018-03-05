using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Web;

namespace ParseHtmlToJson
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
             string rootHTML = htmlFile.Text;
             string rootJSON = jsonFile.Text;
             string[] allHtml = Directory.GetFiles(htmlFile.Text, "*.htm", SearchOption.AllDirectories);
             if(allHtml.Length == 0)
             {
                 allHtml = Directory.GetFiles(htmlFile.Text, "*.html", SearchOption.AllDirectories);
             }
             DirectoryInfo dirMD = new DirectoryInfo(rootHTML);
            label3.Enabled = true;
            progressBar1.Enabled = true;
            progressBar1.Visible = true;
            // Set Minimum to 1 to represent the first file being copied.
            progressBar1.Minimum = 1;
            // Set Maximum to the total number of files to copy.
            progressBar1.Maximum = allHtml.Length;
            // Set the initial value of the ProgressBar.
            progressBar1.Value = 1;
            // Set the Step property to a value of 1 to represent each file being copied.
            progressBar1.Step = 1;
            ProcessFilesAtRoot(dirMD, rootJSON, rootHTML);
            ProcessAllFiles(dirMD, rootJSON, rootHTML);
            MessageBox.Show(allHtml.Length +  " HTML files converted!");
        }

        public void ProcessFilesAtRoot(DirectoryInfo dirMD, string rootJSON, string rootHTML)
        {
            FileInfo[] dirFiles = dirMD.GetFiles("*.html", SearchOption.TopDirectoryOnly);
            if (dirFiles.Length > 0)
            {
                foreach (FileInfo file in dirFiles)
                {
                    string targetFileName = rootJSON + "\\" + file.Name;
                    if (file.FullName.IndexOf("toc.html", StringComparison.OrdinalIgnoreCase) == -1 && file.FullName.Contains(".html"))
                    {
                        int i = 0;
                        HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                        htmlDoc.Load(file.FullName);

                        Newtonsoft.Json.Linq.JObject product = new Newtonsoft.Json.Linq.JObject();
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
                                    value = HttpUtility.HtmlDecode(attribColl[1].Value.ToString());
                                }
                                else
                                {
                                    tag = attribColl[0].Value;
                                    value = attribColl[1].Value.ToString();
                                }
                                product.Add(new Newtonsoft.Json.Linq.JProperty(tag, value));
                            }
                        }

                        HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//body");
                        product.Add(new Newtonsoft.Json.Linq.JProperty("Content", node.InnerText));
                        string jsonFilePath = targetFileName.Replace(".html", ".json");

                        StreamWriter sysfile = File.CreateText(jsonFilePath);
                        using (JsonTextWriter writer = new JsonTextWriter(sysfile))
                        {
                            product.WriteTo(writer);
                            progressBar1.PerformStep();
                        }
                    }
                }
            }
        }
        public void  ProcessAllFiles(DirectoryInfo dirMD, string rootHTML, string rootJSON)
        {
            
            foreach (DirectoryInfo d in dirMD.GetDirectories())
            {
                FileInfo[] dirFiles = d.GetFiles("*.html",SearchOption.AllDirectories);
                if (dirFiles.Length > 0)
                {
                    foreach (FileInfo file in dirFiles)
                    {
                        string targetFileName = CreateTargetDirectory(rootHTML, file.FullName, rootJSON);
                        if (file.FullName.IndexOf("toc.html",StringComparison.OrdinalIgnoreCase) == -1 && file.FullName.Contains(".html"))
                        {
                            int i = 0;
                            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
                            htmlDoc.Load(file.FullName);
                            
                            
                            Newtonsoft.Json.Linq.JObject product = new Newtonsoft.Json.Linq.JObject();
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
                                        value = HttpUtility.HtmlDecode(attribColl[1].Value.ToString());
                                    }
                                    else
                                    {
                                        tag = attribColl[0].Value;
                                        value = attribColl[1].Value.ToString();
                                    }
                                    product.Add(new Newtonsoft.Json.Linq.JProperty(tag, value));
                                }
                            }

                            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//body");
                            product.Add(new Newtonsoft.Json.Linq.JProperty("Content", node.InnerText));
                            string jsonFilePath = targetFileName.Replace(".html", ".json");

                            StreamWriter sysfile = File.CreateText(jsonFilePath);
                            using (JsonTextWriter writer = new JsonTextWriter(sysfile))
                            {
                                product.WriteTo(writer);
                                progressBar1.PerformStep();
                            }
                        }
                    }
                    ProcessAllFiles(d, rootHTML, rootJSON);
                }
            }
            
        }
        static string CreateTargetDirectory(string jsonOutputPath, string fullFilename, string htmlPath)
        {
            string fullPath = jsonOutputPath;
            fullFilename = fullFilename.Substring(htmlPath.Length);
            string[] dirs = fullFilename.Split('\\');
            foreach (string dir in dirs)
            {
                //The root will be "" and the last aelement will be the file name
                if ((dir != "") && (!dir.Contains(".")))
                {
                    fullPath = fullPath + "\\" + dir;
                }
                if (!dir.Contains("."))
                {
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                }
                else
                {
                    //add the file name
                    fullPath = fullPath + "\\" + dir;
                }
            }
            return fullPath;
        }
        
        private void htmlFile_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
           


            if
             (dialog.ShowDialog() == DialogResult.OK)
            {
                htmlFile.Text = dialog.SelectedPath;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if
             (dialog.ShowDialog() == DialogResult.OK)
            {
                jsonFile.Text = dialog.SelectedPath;
            }
        }

        private void jsonFile_TextChanged(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }
    }

}
