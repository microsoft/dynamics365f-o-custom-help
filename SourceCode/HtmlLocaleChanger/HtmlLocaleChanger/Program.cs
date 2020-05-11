using CommandLine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlLocaleChanger
{
    public class Options
    {
        [Option('h', "htmlDirectory", Required = true, HelpText = "Directory containing the HTML files.")]
        public string HtmlDirectory { get; set; }

        [Option('l', "locale", Required = true, HelpText = "New locale for the HTML files.")]
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
                CultureInfo ci = new CultureInfo(opts.Locale);
                if (!CultureInfo.GetCultures(CultureTypes.AllCultures).Contains(ci))
                {
                    throw new CultureNotFoundException();
                }
            }
            catch (CultureNotFoundException)
            {
                Console.WriteLine("Locale " + opts.Locale + " is not supported by this system. Do you want to proceed? (Y/N)");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    return;
                }
            }
            HTMLDirectory htmlDirectory = new HTMLDirectory(opts.HtmlDirectory);
            LocaleChangeProcessor processor = new LocaleChangeProcessor(htmlDirectory, opts.Locale, opts.Verbose);
            processor.Process();
        }
    }
}
