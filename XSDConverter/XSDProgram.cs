using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using XSDService;
using System.Text.Json;

namespace XSDConverter
{
    sealed internal class XSDProgram
    {
        private static XsdToJson xsdLib;
        static void Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);
            if (args.Length > 0)
            {
                if (args.Length == 1)
                {
                    var source = args[0];
                    var fileInfo = new FileInfo(source);
                    if (File.Exists(source) && fileInfo.Extension.Equals(".xsd"))
                    {
                        try
                        {
                            var streamReader = new StreamReader(source);
                            xsdLib = new XsdToJson(streamReader);
                            xsdLib.Convert();
                            Console.WriteLine(xsdLib.SchemaJson);
                        }
                        catch(JsonException ex)
                        {
                            Console.Error.WriteLine($"Much nested node structure found. {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine(ex.ToString());
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("No file exist");
                    }
                }
            }
            else
            {
                Console.Error.WriteLine("No Argument Found");
            }
        }
    }

    sealed internal class CommandLineParser
    {
        public CommandLineParser()
        {
            Arguments = new Dictionary<string, string[]>();
        }
        public IDictionary<string, string[]> Arguments { get; private set; }
        public void Parse(string[] args)
        {
            var currentName = "";
            var values = new List<string>();
            foreach (var arg in args)
            {
                if (arg.StartsWith("/"))
                {
                    if (currentName != "")
                        Arguments[currentName] = values.ToArray();
                    values.Clear();
                    currentName = arg.Substring(1);
                }
                else if (currentName == "")
                    Arguments[arg] = new string[0];
                else
                    values.Add(arg);
            }
            if (currentName != "")
                Arguments[currentName] = values.ToArray();
        }
        public bool Contains(string name)
        {
            return Arguments.ContainsKey(name);
        }
    }
}
