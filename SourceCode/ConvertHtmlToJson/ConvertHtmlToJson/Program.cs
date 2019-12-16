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

        [Option('l', "locale", Required = true, HelpText = "Locale of the HTML source files. JSON files must contain the locale in order for search to support the locale.")]
        public string Locale { get; set; }

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
                //HelpLocale helpLocale = new HelpLocale(opts.locale);
                //CultureInfo cultureInfo = helpLocale.GetCultureInfo();
                //Console.WriteLine(cultureInfo.DisplayName);

                JSONDirectory jsonDirectory = new JSONDirectory(opts.JsonDirectory);
                HTMLDirectory htmlDirectory = new HTMLDirectory(opts.HtmlDirectory);

                HTMLtoJSONProcessor processor = new HTMLtoJSONProcessor(htmlDirectory, jsonDirectory, opts.Verbose);
                processor.Process();
            }
            catch (CultureNotFoundException)
            {
                Console.WriteLine(opts.Locale + " is not a supported locale.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
