using System;
using System.IO;
using CommandLine;
using Newtonsoft.Json.Linq;

namespace Chomp
{
    class Program
    {
        private static string OutputDirectory { get; set; }
        public class Options
        {
            [Option('i', "input", Required = true, HelpText = "Input file.")]
            public string InputFile { get; set; }

            [Option('o', "output", Required = false, HelpText = "Output directory.")]
            public string OutputDirectory { get; set; }
        }

        static string GetFileName(string url)
        {
            Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);

            if(!uri.IsAbsoluteUri)
            {
                uri = new Uri(new Uri("http://example.com"), url);
            }

            string fileName = uri.Segments[uri.Segments.Length - 1];
            return fileName;
        }

        static void ProcessFile(JObject item)
        {
            string fileName = GetFileName(item["url"].ToString());
            string text = item["text"].ToString();
            JArray ranges = (JArray)item["ranges"];

            for (var i = ranges.Count - 1; i >= 0; i--)
            {
                int start = (int)ranges[i]["start"];
                int end = (int)ranges[i]["end"];

                string before = text.Substring(0, start);
                string after = text.Substring(end);
                text = before + after;
            }

            FileInfo file = new System.IO.FileInfo(Path.Join(OutputDirectory, fileName));
            file.Directory.Create();
            File.WriteAllText(file.FullName, text);
        }

        static void ReadFile(Options options)
        {
            JArray json = new JArray();

            try
            {
                json = JArray.Parse(File.ReadAllText(options.InputFile));
            }

            catch(Exception error)
            {
                Console.WriteLine(error.Message);
                Environment.Exit(-1);
            }

            foreach (JObject item in json)
            {
                ProcessFile(item);
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(options =>
            {
                OutputDirectory = options.OutputDirectory ?? "output";
                ReadFile(options);
            });
        }
    }
}
