using System.Diagnostics;
using System.Xml.Linq;
using Castle.Core.Internal;

namespace PresentationCore.Tests.TargetFramework.Tests
{
    public class OlderTFMTests
    {
        private readonly List<string> projectFiles = new List<string>();
        private string _projectName;
        [Theory]
        //[InlineData("net5.0")] - //out of support
        [InlineData("net6.0")]
        //[InlineData("net7.0")] - //out of support
        [InlineData("net8.0")]
        [InlineData("net9.0")]
        //[InlineData("netcoreapp3.0")] - //out of support
        //[InlineData("netcoreapp3.1")] - //out of support
        public void OlderTFMTestFX(string targetFramework)
        {
            if (targetFramework != null)
            {
                Assert.True(true);
            }
            Console.WriteLine($"Starting test with target framework: {targetFramework}");
            try
            {
                targetFramework.Should().NotBeNullOrEmpty();

                if (_projectName.IsNullOrEmpty())
                {
                    _projectName = "WPFSampleApp" + targetFramework;
                }

                CreateProject(projectFiles, targetFramework, _projectName);

                // Verify if the projects are created
                foreach (var projectFile in projectFiles)
                {
                    string fullPath = Path.Combine(projectFile, $"{_projectName}.csproj");
                    bool check = File.Exists(fullPath);
                    Assert.True(check);

                    // Read the project file
                    //publish                    

                    string publishPath = projectFile + "\\publish";
                    RunDotnetPublish(fullPath, publishPath);
                    string fullPathPublish = Path.Combine(publishPath, $"{_projectName}.exe");
                    bool checkPub = File.Exists(fullPathPublish);
                    Assert.True(checkPub);
                    bool checklaunch = LaunchWPFApp(fullPathPublish);
                    if(checklaunch)
                    {
                        Assert.True(true);
                    }
                    else
                    {
                        Assert.True(false);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
        private static void CreateProject(List<string> projectFiles, string? targetFramework, string projectName)
        {
            // Define project path
            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "TFMProjects", "WPFSampleApp" + targetFramework);

            // Clean up the directory if it exists
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, true);
            }

            Directory.CreateDirectory(rootPath);

            string projectPath = Path.Combine(rootPath, projectName);

            // Create project directory
            Directory.CreateDirectory(projectPath);

            projectFiles.Add(projectPath);

            //// Check if the required SDK is installed
            //string requiredSdkVersion = "6.0.0"; // Replace with the required SDK version
            //bool isSdkInstalled = IsSdkInstalled(requiredSdkVersion);
            //if (!isSdkInstalled)
            //{
            //    Console.WriteLine($"Required .NET SDK version {requiredSdkVersion} is not installed. Installing now...");

            //    // PowerShell script to install the latest .NET SDK version
            //    string installScript = @"[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
            //.\dotnet-install.ps1 -Channel STS";

            //    // Start the PowerShell process to run the script
            //    ProcessStartInfo psStartInfo = new ProcessStartInfo
            //    {
            //        FileName = "powershell.exe",
            //        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{installScript}\"",
            //        RedirectStandardOutput = true,
            //        RedirectStandardError = true,
            //        UseShellExecute = false,
            //        CreateNoWindow = true
            //    };

            //    using (Process? psProcess = Process.Start(psStartInfo))
            //    {
            //        if (psProcess == null)
            //        {
            //            Console.WriteLine("Failed to start the PowerShell process.");
            //            return;
            //        }
            //        psProcess.WaitForExit();
            //        string psOutput = psProcess.StandardOutput.ReadToEnd();
            //        string psError = psProcess.StandardError.ReadToEnd();

            //        Console.WriteLine(psOutput);
            //        if (!string.IsNullOrEmpty(psError))
            //        {
            //            Console.WriteLine($"PowerShell error: {psError}");
            //        }

            //        if (psProcess.ExitCode != 0)
            //        {
            //            throw new Exception("Failed to install the .NET SDK.");
            //        }
            //    }

            //    // Re-check if the required SDK is installed
            //    isSdkInstalled = IsSdkInstalled(requiredSdkVersion);
            //    if (!isSdkInstalled)
            //    {
            //        throw new Exception($"Failed to install the required .NET SDK version {requiredSdkVersion}.");
            //    }
            //}

            // Command to create a new project
            string createProjectCommand = $"dotnet new wpf -n {projectName} --framework {targetFramework}";

            // Start the process to create the project
            ProcessStartInfo startInfo = new("cmd", "/c " + createProjectCommand)
            {
                WorkingDirectory = rootPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

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

            //// Verify if the project was created successfully
            //string projectFilePath = Path.Combine(projectPath, $"{projectName}.csproj");
            //if (File.Exists(projectFilePath))
            //{
            //    Console.WriteLine($"Project created successfully: {projectFilePath}");
            //}
            //else
            //{
            //    throw new Exception($"Project creation failed: {projectFilePath} not found.");
            //}

            //// Attempt to restore the project
            //string restoreCommand = $"dotnet restore \"{projectFilePath}\"";
            //ProcessStartInfo restoreStartInfo = new("cmd", "/c " + restoreCommand)
            //{
            //    WorkingDirectory = rootPath,
            //    RedirectStandardOutput = true,
            //    RedirectStandardError = true,
            //    UseShellExecute = false,
            //    CreateNoWindow = true
            //};

            //using (Process? restoreProcess = Process.Start(restoreStartInfo))
            //{
            //    if (restoreProcess == null)
            //    {
            //        Console.WriteLine("Failed to start the restore process.");
            //        return;
            //    }
            //    restoreProcess.WaitForExit();
            //    string restoreOutput = restoreProcess.StandardOutput.ReadToEnd();
            //    string restoreError = restoreProcess.StandardError.ReadToEnd();

            //    Console.WriteLine(restoreOutput);
            //    if (!string.IsNullOrEmpty(restoreError))
            //    {
            //        Console.WriteLine($"Restore error: {restoreError}");
            //    }

            //    if (restoreProcess.ExitCode != 0)
            //    {
            //        throw new Exception("Project restore failed.");
            //    }
            //}
        }

        //private static void CreateProject(List<string> projectFiles, string? targetFramework, string projectName)
        //{
        //    // Define project path
        //    ////string rootPath = "D:\\FORK\\Harshita\\TFM\\wpf\\src\\Microsoft.DotNet.Wpf\\src\\WPFSampleApp";
        //    string rootPath = Directory.GetCurrentDirectory() + "\\TFMProjects\\WPFSampleApp" + targetFramework;
        //    // Clean up the directory if it exists
        //    //if (Directory.Exists(rootPath))
        //    //{
        //    //    Directory.Delete(rootPath, true);
        //    //}

        //    Directory.CreateDirectory(rootPath);

        //    string projectPath = Path.Combine(rootPath, projectName);


        //    // Create project directory
        //    Directory.CreateDirectory(projectPath);

        //    projectFiles.Add(projectPath);

        //    // Command to create a new project
        //    string createProjectCommand = $"dotnet new wpf -n {projectName} --framework {targetFramework}";

        //    // Start the process to create the project
        //    ProcessStartInfo startInfo = new("cmd", "/c " + createProjectCommand)
        //    {
        //        WorkingDirectory = projectPath,
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    using (Process? process = Process.Start(startInfo))
        //    {
        //        if (process == null)
        //        {
        //            Console.WriteLine("Failed to start the process.");
        //            return;
        //        }
        //        process.WaitForExit();
        //        string output = process.StandardOutput.ReadToEnd();
        //        string error = process.StandardError.ReadToEnd();

        //        Console.WriteLine(output);
        //        if (!string.IsNullOrEmpty(error))
        //        {
        //            Console.WriteLine($"Error: {error}");
        //        }
        //    }
        //}

        private static void RunDotnetPublish(string projectPath, string publishPath)
        {
            // Create project directory
            Directory.CreateDirectory(publishPath);

            if (Directory.Exists(publishPath))
            {
                Assert.True(true);
            }
            else
            {
                Assert.True(false);
            }
                // Set up the process start information to run the 'dotnet publish' command
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish \"{projectPath}\" -c Release -r win-x64 -o \"{publishPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };           

            // Start the process
            using var process = Process.Start(startInfo);
            if (process != null)
            {
                // Read output and error streams
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // Optionally log output and errors
                Console.WriteLine(output);
                Console.WriteLine(error);
            }
        }

        private static bool LaunchWPFApp(string filename)
        {
            string? publishPath = null;
            string? exePath = null;
            
            string? directoryPath = Path.GetDirectoryName(filename);
            string? withoutExtension = Path.GetFileNameWithoutExtension(filename);
            if (directoryPath != null)
            {
                publishPath = directoryPath;
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

            // Wait for a moment to ensure the app is up
            Thread.Sleep(1000);

            // Close the main window of the process
            if (!process.CloseMainWindow())
            {
                // If the main window could not be closed, kill the process
                process.Kill();
            }
            // Wait for the process to exit
            process.WaitForExit();

            // Check if the process has exited
            Assert.True(process.HasExited);

            return process.ExitCode == 0;
            
        }
        private static bool IsSdkInstalled(string requiredSdkVersion)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--list-sdks",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null)
                {
                    throw new Exception("Failed to start the process to list SDKs.");
                }

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains(requiredSdkVersion);
            }
        }
        private static void ChangeTargetFramework(string WpfProjectPath)
        {
        }
    }
}
