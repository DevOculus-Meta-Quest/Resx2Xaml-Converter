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
            Console.WriteLine("Program started.");

            string directory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            Console.WriteLine($"Working directory: {directory}");

            var resxFiles = Directory.EnumerateFiles(directory, "*.resx", SearchOption.AllDirectories).ToList();
            Console.WriteLine($"Found {resxFiles.Count} .resx files.");

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
            Console.WriteLine($"An error occurred in Main: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }

    static void ProcessResxFile(string resxFile)
    {
        try
        {
            Console.WriteLine($"Processing file: {resxFile}");

            XDocument resxDocument = XDocument.Load(resxFile);
            string xamlFileName = Path.ChangeExtension(resxFile, "_converted.xaml");

            XNamespace xmlns = "http://schemas.microsoft.com/winfx/2006/xaml";
            XNamespace sysNs = "clr-namespace:System;assembly=mscorlib";

            XElement rootElement = new XElement("ResourceDictionary",
                new XAttribute(XNamespace.Xmlns + "x", xmlns),
                new XAttribute(XNamespace.Xmlns + "sys", sysNs));

            foreach (XElement dataElement in resxDocument.Descendants("data"))
            {
                string nameAttribute = dataElement.Attribute("name")?.Value;
                string valueElement = dataElement.Element("value")?.Value;

                if (!string.IsNullOrEmpty(nameAttribute) && !string.IsNullOrEmpty(valueElement))
                {
                    Console.WriteLine($"Processing key: {nameAttribute}");

                    string sanitizedKey = SanitizeKey(nameAttribute);
                    XElement xamlElement = new XElement(sysNs + "String",
                        new XAttribute(xmlns + "Key", sanitizedKey),
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
        // Replace invalid characters with underscores
        return string.Join("_", key.Split(Path.GetInvalidFileNameChars()))
            .Replace(".", "_")  // Replace dots with underscores
            .Replace("$", "");  // Remove dollar signs
    }
}