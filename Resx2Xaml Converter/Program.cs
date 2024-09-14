using System;
using System.IO;
using System.Xml.Linq;

class Program
{
    static void Main()
    {
        // Set the working directory to one level above the current directory (publish folder)
        string directory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
        Console.WriteLine($"Processing .resx files in the directory: {directory}");

        string[] resxFiles = Directory.GetFiles(directory, "*.resx");

        if (resxFiles.Length == 0)
        {
            Console.WriteLine($"No .resx files found in the directory: {directory}");
            return;
        }

        foreach (string resxFile in resxFiles)
        {
            try
            {
                // Load the .resx file
                XDocument resxDocument = XDocument.Load(resxFile);

                // Create a sanitized XAML file name based on the .resx file
                string xamlFileName = Path.Combine(directory, SanitizeFileName(Path.GetFileNameWithoutExtension(resxFile)) + "_converted.xaml");

                // Create the root element for the XAML file
                XElement rootElement = new XElement("ResourceDictionary");

                // Loop through each element in the .resx file
                foreach (XElement dataElement in resxDocument.Descendants("data"))
                {
                    string nameAttribute = dataElement.Attribute("name")?.Value;

                    if (nameAttribute != null)
                    {
                        // Add the resource as a XAML key-value pair with sanitized name
                        XElement xamlElement = new XElement("System:String",
                            new XAttribute("x:Key", SanitizeFileName(nameAttribute)),
                            dataElement.Element("value")?.Value);
                        rootElement.Add(xamlElement);
                    }
                }

                // Ensure the parent directory of the XAML file exists
                string outputDirectory = Path.GetDirectoryName(xamlFileName);
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                // Save the XAML file
                XDocument xamlDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), rootElement);
                xamlDocument.Save(xamlFileName);

                Console.WriteLine($"Converted {resxFile} to {xamlFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {resxFile}: {ex.Message}");
            }
        }

        Console.WriteLine("Conversion complete.");
    }

    // Function to sanitize file names and keys by replacing invalid characters
    static string SanitizeFileName(string name)
    {
        // Replace invalid characters (like ':') with underscores for both file names and keys
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // Add custom characters like ':' which need to be removed even if they are valid in some cases
        char[] customInvalidChars = { ':', '/', '\\' };

        foreach (char invalidChar in invalidChars)
        {
            name = name.Replace(invalidChar, '_');
        }

        foreach (char customChar in customInvalidChars)
        {
            name = name.Replace(customChar, '_');
        }

        return name;
    }
}
