using System;
using System.Collections.Generic;
using System.IO;
using XSDLib;

namespace XSDConverter
{
    sealed internal class XSDProgram
    {
        private const string _paramSource="source";
        private const string _paramOutputType="outputType";
        private static CommandLineParser commandLineParser;
        private static XSDLib.XSDLib xsdLib;
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                commandLineParser = new CommandLineParser();
                commandLineParser.Parse(args);
                if (commandLineParser.Arguments.ContainsKey(_paramSource))
                {
                    var source = commandLineParser.Arguments[_paramSource][0];

                    var fileInfo = new FileInfo(source);
                    if (File.Exists(source) && fileInfo.Extension.Equals(".xsd"))
                    {
                        xsdLib = new XSDLib.XSDLib(source);
                        xsdLib.Convert();
                        if (commandLineParser.Arguments.ContainsKey(_paramOutputType))
                        {
                            var target = commandLineParser.Arguments[_paramOutputType][0];
                            if ("json" == target.ToLowerInvariant())
                            {
                                File.AppendAllText(fileInfo.FullName.Replace(".xsd", ".json"), xsdLib.SchemaJson);
                            }
                            else
                            {
                                File.AppendAllText(fileInfo.FullName.Replace(".xsd", ".xml"), xsdLib.SchemaXML);

                            }
                        }
                        else
                        {
                            File.AppendAllText(fileInfo.FullName.Replace(".xsd", ".xml"), xsdLib.SchemaXML);
                        }
                    }
                }
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
