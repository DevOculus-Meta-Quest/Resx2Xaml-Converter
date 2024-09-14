using System.Xml.Linq;

static void ProcessResxFile(string resxFile)
{
    try
    {
        Console.WriteLine($"Processing file: {resxFile}");

        XDocument resxDocument = XDocument.Load(resxFile);
        string xamlFileName = Path.ChangeExtension(resxFile, "_converted.xaml");

        XElement rootElement = new XElement("ResourceDictionary",
            new XAttribute(XNamespace.Xmlns + "x", "http://schemas.microsoft.com/winfx/2006/xaml"),
            new XAttribute("xmlns", "clr-namespace:System;assembly=mscorlib"));

        foreach (XElement dataElement in resxDocument.Descendants("data"))
        {
            string nameAttribute = dataElement.Attribute("name")?.Value;
            string valueElement = dataElement.Element("value")?.Value;

            if (!string.IsNullOrEmpty(nameAttribute) && !string.IsNullOrEmpty(valueElement))
            {
                Console.WriteLine($"Processing key: {nameAttribute}");

                string sanitizedKey = SanitizeKey(nameAttribute);
                XElement xamlElement = new XElement("sys:String",
                    new XAttribute(XNamespace.Xmlns + "sys", "clr-namespace:System;assembly=mscorlib"),
                    new XAttribute(XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml"), sanitizedKey),
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
    return string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
}