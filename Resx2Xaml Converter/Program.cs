using System;
using System.IO;
using System.Xml.Linq;
using System.Linq;

class Program
{
    static void Main()
    {
        try
        {
            string directory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            Console.WriteLine($"Processing .resx files in the directory: {directory}");

            var resxFiles = Directory.EnumerateFiles(directory, "*.resx", SearchOption.AllDirectories);

            if (!resxFiles.Any())
            {
                Console.WriteLine($"No .resx files found in the directory: {directory}");
                return;
            }

            foreach (string resxFile in resxFiles)
            {
                ProcessResxFile(resxFile);
            }

            Console.WriteLine("Conversion complete.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static void ProcessResxFile(string resxFile)
    {
        try
        {
            Console.WriteLine($"Processing file: {resxFile}");

            XDocument resxDocument = XDocument.Load(resxFile);
            string xamlFileName = Path.ChangeExtension(resxFile, "_converted.xaml");

            XElement rootElement = new XElement("ResourceDictionary",
                new XAttribute(XNamespace.Xmlns + "x", "http://schemas.microsoft.com/winfx/2006/xaml"));

            foreach (XElement dataElement in resxDocument.Descendants("data"))
            {
                string nameAttribute = dataElement.Attribute("name")?.Value;
                string valueElement = dataElement.Element("value")?.Value;

                if (!string.IsNullOrEmpty(nameAttribute) && !string.IsNullOrEmpty(valueElement))
                {
                    Console.WriteLine($"Processing key: {nameAttribute}");

                    XElement xamlElement = new XElement("System:String",
                        new XAttribute("x:Key", SanitizeKey(nameAttribute)),
                        valueElement);
                    rootElement.Add(xamlElement);
                }
                else
                {
                    Console.WriteLine($"Skipping invalid resource: {nameAttribute}");
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(xamlFileName));

            XDocument xamlDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), rootElement);
            xamlDocument.Save(xamlFileName);

            Console.WriteLine($"Converted {resxFile} to {xamlFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing {resxFile}: {ex.Message}");
        }
    }

    static string SanitizeKey(string key)
    {
        return new string(key.Select(ch => char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_').ToArray());
    }
}