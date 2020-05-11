using CommandLine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertHtmlToJson
{
    public class Options
    {
        [Option('h', "htmlDirectory", Required = true, HelpText = "Directory containing the HTML source files.")]
        public string HtmlDirectory { get; set; }

        [Option('j', "jsonDirectory", Required = true, HelpText = "Directory where the JSON files should be created.")]
        public string JsonDirectory { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Detailed logging for each file processed.")]
        public bool Verbose { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunOptions(opts));
        }

        private static void RunOptions(Options opts)
        {
            try
            {
                JSONDirectory jsonDirectory = new JSONDirectory(opts.JsonDirectory);
                HTMLDirectory htmlDirectory = new HTMLDirectory(opts.HtmlDirectory);

                HTMLtoJSONProcessor processor = new HTMLtoJSONProcessor(htmlDirectory, jsonDirectory, opts.Verbose);
                if (processor.Process() > 0)
                {
                    Console.WriteLine("Please fix all of the errors listed and rerun ConvertHtmlToJson.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
