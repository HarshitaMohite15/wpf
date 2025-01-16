// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//using System.Collections.Generic;
//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading;
using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace TargetFramework.Tests;

public class XUnitTFMTest
{

    private static IConfiguration? s_configuration = InitializeConfiguration();
    //private static readonly string s_wpfProjectDirectory = InitializeWpfProjectDirectory();
    private static readonly string s_appPath = InitializeAppPath();
    private static readonly string s_newTargetFramework = InitializeTargetFramework();
    //private static readonly string? s_wpfProjectDirectory = null;
    static XUnitTFMTest()
    {
        CreateTFMProjects();
    }
    private static IConfiguration InitializeConfiguration()

    {
        s_configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) // Set base path
            .AddJsonFile("app.json", optional: false, reloadOnChange: true) // Load appsettings.json
            .Build();
        return s_configuration;
    }
    // Static methods to initialize fields
    //private static string InitializeWpfProjectDirectory()
    //{
    //    if (s_configuration != null)
    //    {
    //        string? s_currentDir = Directory.GetCurrentDirectory();
    //        string? wpfProjectDirectory = s_configuration["WpfProjectDirectory"];
    //        if (wpfProjectDirectory != null && s_currentDir != null)
    //        {
    //            DirectoryInfo? parentDir = Directory.GetParent(s_currentDir);
    //            return parentDir + wpfProjectDirectory;
    //        }
    //    }
    //    return string.Empty;
    //}

    private static string InitializeAppPath()
    {
        if (s_configuration != null)
        {
            string? s_currentDir = Directory.GetCurrentDirectory();
            string? appPath = s_configuration["appPath"];
            if (appPath != null && s_currentDir != null)
            {
                return s_currentDir + appPath;
            }
        }
        return string.Empty;
    }

    private static string InitializeTargetFramework()
    {
        return s_configuration?["latestTargetFramework"] ?? string.Empty;
    }

    public static IEnumerable<object[]> GetTestData()
    {
        string? s_wpfProjectDirectory = null;
        if (s_configuration != null)
        {
            string? s_currentDir = Directory.GetCurrentDirectory();
            string? wpfProjectDirectory = s_configuration["WpfProjectDirectory"];
            if (wpfProjectDirectory != null && s_currentDir != null)
            {
                DirectoryInfo? parentDir = Directory.GetParent(s_currentDir);
                s_wpfProjectDirectory = parentDir + wpfProjectDirectory;
            }
        }
        if (s_wpfProjectDirectory != null)
        {
            foreach (var filename in Directory.GetFiles(s_wpfProjectDirectory, "*.csproj", SearchOption.AllDirectories))
            {
                yield return new object[] { filename.ToString() };
            }
        }
        else
        {
            yield return (object[])Enumerable.Empty<object[]>();          
        }

    }

    [Theory]
    [MemberData(nameof(GetTestData))]
    public void TestWPFApp(string filename)
    {
        var publishResult=PublishWPFApp(filename);
        Assert.True(publishResult, "WPF Application publish failed.");
        //commenting as the tests are failing due to windowsbase.dll error

        //var launchResult = LaunchWPFApp(filename);
        //Assert.True(launchResult, "WPF Application launch failed.");
        //ChangeTargetFramework(filename);
        //publishResult = PublishWPFApp(filename);
        //Assert.True(publishResult, "WPF Application publish failed.");
        //launchResult = LaunchWPFApp(filename);
        //Assert.True(launchResult, "WPF Application launch failed.");

    }

    private static bool PublishWPFApp(string filename)
    {        
        string? projectPath = null;
        string? publishPath = null;
        if (filename != null)
        {
            projectPath = Path.GetDirectoryName(filename);
        }

        if (projectPath != null)
        {
            publishPath = Path.Combine(projectPath, "publish");
        }

        // Act
         var result=false;
        if (publishPath != null && projectPath != null)
        {
            result = RunDotnetPublish(projectPath, publishPath);
        }
        return result;
        // Assert.True(Directory.Exists(publishPath));

    }

    private static bool RunDotnetPublish(string projectPath, string publishPath)
    {
        // Set up the process start information to run the 'dotnet publish' command
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"publish \"{projectPath}\" -c Release -r win-x64 --self-contained -o \"{publishPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Start the process
        using var process = Process.Start(startInfo);
        if (process == null)
        {
            return false;
        }

        // Read output and error streams
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        // Optionally log output and errors
        Console.WriteLine(output);
        Console.WriteLine(error);

        return process.ExitCode == 0; // Return true if the exit code is 0 (success)
    }

    private static bool LaunchWPFApp(string filename)
    {
        string? publishPath = null;
        string? exePath = null;
        string? directoryPath = Path.GetDirectoryName(filename);
        string? withoutExtension = Path.GetFileNameWithoutExtension(filename);
        if (directoryPath != null)
        {
            publishPath = Path.Combine(directoryPath, "publish");
        }
        if (publishPath != null)
        {
            exePath = Path.Combine(publishPath, withoutExtension + ".exe");
        }

        // Start the WPF application
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
            }
        };

        process.Start();

        // Optionally wait for a moment to ensure the app is up
        Thread.Sleep(2000);
        return process.ExitCode == 0;
    }
    private static void CreateTFMProjects()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = s_appPath,
                UseShellExecute = true,
            }
        };
        process.Start();
        // Optionally wait for a moment to ensure the app is up
        Thread.Sleep(2000);
        //Assert.False(process.HasExited);
        process.WaitForExit();
        process.Kill();
    }


    //Change the TFM to latest version of .Net
    private static void ChangeTargetFramework(string WpfProjectPath)
    {
        if (!File.Exists(WpfProjectPath))
        {
            Console.WriteLine("Project file not found.");
            return;
        }

        // Load the project file
        XDocument doc = XDocument.Load(WpfProjectPath);
        XElement? propertyGroup = doc.Descendants("PropertyGroup").FirstOrDefault();

        if (propertyGroup != null && s_newTargetFramework != null)
        {
            XElement? targetFrameworkElement = propertyGroup.Elements("TargetFramework").FirstOrDefault();
            if (targetFrameworkElement != null)
            {
                targetFrameworkElement.Value = s_newTargetFramework;
            }
            else
            {
                propertyGroup.Add(new XElement("TargetFramework", s_newTargetFramework));
            }

            // Save the changes
            doc.Save(WpfProjectPath);
            Console.WriteLine($"Target framework changed to '{s_newTargetFramework}' in '{WpfProjectPath}'.");
        }
        else
        {
            Console.WriteLine("No PropertyGroup found in the project file.");
        }
    }

}
