// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using System.Configuration;
using System.Diagnostics;
using System.Xml.Linq;



string projectName;
string targetFramework;
string[] tfmName;

List<string>? allTFMs = null;

string? tfmsValue = ConfigurationManager.AppSettings["tfms"];

if (string.IsNullOrEmpty(tfmsValue))
{
    Console.WriteLine("The 'tfms' key is missing or has no value.");
}
else
{
    allTFMs = [.. tfmsValue.Split(';')];
}
if (allTFMs != null)
{
    if (allTFMs.Count > 0)
    {
        string rootPath = Directory.GetCurrentDirectory() + "TFMProjects";
        if ((Directory.Exists(rootPath)))
        {
            Directory.Delete(rootPath, true);
            Directory.CreateDirectory(rootPath);
        }
        for (int i = 0; i < allTFMs.Count; i++)
        {
            tfmName = allTFMs[i].Split('.');
            projectName = "TestSampleWPF" + tfmName[0].ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture);
            targetFramework = allTFMs[i];
            // Create the project
            CreateProject(projectName, targetFramework);
        }
    }
}



static void CreateProject(string projectName, string targetFramework)
{
    // Define project path
    string rootPath = Directory.GetCurrentDirectory() + "TFMProjects";
    if (!(Directory.Exists(rootPath)))
    {
        Directory.CreateDirectory(rootPath);
    }


    string projectPath = Path.Combine(rootPath, projectName);

    // Create project directory
    Directory.CreateDirectory(projectPath);

    // Command to create a new project
    string createProjectCommand = $"dotnet new wpf -n {projectName} --framework {targetFramework}";

    // Start the process to create the project
    ProcessStartInfo startInfo = new("cmd", "/c " + createProjectCommand)
    {
        WorkingDirectory = projectPath,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    // Check if startInfo is null
    if (startInfo == null)
    {
        Console.WriteLine("ProcessStartInfo is null, cannot start the process.");
        return;
    }

    using (Process? process = Process.Start(startInfo))
    {
        if (process == null)
        {
            Console.WriteLine("Failed to start the process.");
            return;
        }
        process.WaitForExit();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        Console.WriteLine(output);
        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"Error: {error}");
        }
    }

    Console.WriteLine($"Project '{projectName}' created with target framework '{targetFramework}'.");
    //// Path to the .csproj file
    //string csprojFilePath = Path.Combine(projectPath, $"{Path.GetFileName(projectPath)}.csproj");

    //if (File.Exists(csprojFilePath))
    //{
    //    // Load the .csproj file as an XML document
    //    XDocument csprojDoc = XDocument.Load(csprojFilePath);

    //    // Check if the WindowsBase reference exists, if not, add it
    //    var itemGroup = csprojDoc.Descendants("ItemGroup").FirstOrDefault();
    //    if (itemGroup != null && !itemGroup.Descendants("Reference").Any(r => r.Attribute("Include")?.Value == "WindowsBase"))
    //    {
    //        // Add the WindowsBase reference manually
    //        itemGroup.Add(new XElement("Reference", new XAttribute("Include", "WindowsBase")));
    //    }

    //    // Save the changes back to the .csproj file
    //    csprojDoc.Save(csprojFilePath);
    //}
}
