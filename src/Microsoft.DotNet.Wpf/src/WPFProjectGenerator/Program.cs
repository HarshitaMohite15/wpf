﻿// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using System.Configuration;
using System.Diagnostics;



string projectName;
string targetFramework;
string[] tfmName;
//string projectName = "TestSampleWPFNET8";
//string targetFramework = "net8.0";
var value = "net5.0;net6.0;net7.0;net8.0;net9.0" ?? string.Empty;
var allTFMs = new List<string>(value.Split(';'));

//var allTFMs = new List<string>(ConfigurationManager.AppSettings["tfms"].Split(';'));

if (allTFMs.Count > 0)
{
    for (int i = 0; i < allTFMs.Count; i++)
    {
        tfmName = allTFMs[i].Split('.');
        projectName = "TestSampleWPF" + tfmName[0].ToString().ToUpper();
        targetFramework = allTFMs[i];
        // Create the project
        CreateProject(projectName, targetFramework);
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
    ProcessStartInfo startInfo = new ProcessStartInfo("cmd", "/c " + createProjectCommand)
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
}
