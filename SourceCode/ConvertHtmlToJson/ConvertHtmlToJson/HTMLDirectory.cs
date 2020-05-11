using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertHtmlToJson
{
    class HTMLDirectory
    {
        private DirectoryInfo htmlDirectory;

        public HTMLDirectory(String htmlPath)
        {
            this.htmlDirectory = new DirectoryInfo(htmlPath);
        }

        public DirectoryInfo GetDirectoryInfo()
        {
            return this.htmlDirectory;
        }
    }
}
