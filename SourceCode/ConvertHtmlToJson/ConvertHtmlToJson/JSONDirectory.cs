using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertHtmlToJson
{
    class JSONDirectory
    {
        private DirectoryInfo jsonDirectory;

        public JSONDirectory(String jsonPath)
        {
            this.jsonDirectory = new DirectoryInfo(jsonPath);
        }

        public DirectoryInfo GetDirectoryInfo()
        {
            return this.jsonDirectory;
        }
    }
}
