using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace TargetFramework.Tests
{
    [TestCaseOrderer(
    ordererTypeName: "TargetFramework.Tests.AlphabeticalOrderer",
    ordererAssemblyName: "TargetFramework.Tests")]
    public class XUnitTest : IClassFixture<ConfigurationFixture>
    {
        private readonly IConfiguration? _configuration;
        private static string? s_wpfProjectDirectory;
        private readonly string? _appPath;
        private static string? s_newTargetFramework;
        private readonly string? _currentDir;
        public XUnitTest(ConfigurationFixture configFixture)
        {
            _configuration = configFixture.Configuration;
            if (_configuration != null)
            {
                _currentDir = Directory.GetCurrentDirectory();
                s_wpfProjectDirectory = _configuration["WpfProjectDirectory"];
                if (s_wpfProjectDirectory != null && _currentDir != null)
                {
                    DirectoryInfo? parentDir = Directory.GetParent(_currentDir);
                    s_wpfProjectDirectory = parentDir + s_wpfProjectDirectory;
                }
                _appPath = _configuration["appPath"];
                if (_appPath != null)
                {
                    _appPath = _currentDir + _appPath;
                }
                s_newTargetFramework = _configuration["latestTargetFramework"];
            }
        }

        //Create projects with different TFMs
        [Fact]
        public void A_StartTest()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _appPath,
                    UseShellExecute = true,
                }
            };
            process.Start();
            // Optionally wait for a moment to ensure the app is up
            Thread.Sleep(2000);
            Assert.False(process.HasExited);
            process.WaitForExit();
            process.Kill();
        }

        //Publish the projects created with different TFMs
        [Theory]
        [MemberData(nameof(GetTestCases))]
        public void B_Publish(string filename)
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

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"publish \"{projectPath}\" -c Release -o \"{publishPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                return;
            }
            // Read the output
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();
            Assert.Equal(0, process.ExitCode);
            Assert.Empty(error);
        }

        //Run the projects created with different TFMs
        [Theory]
        [MemberData(nameof(GetTestCases))]
        public void C_Launch(string filename)
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

            Assert.False(process.HasExited);
            // Optionally, you can close the application after the test
            process.Kill();
        }

        //Publish the projects created with different TFMs after changing the to latest TFM
        [Theory]
        [MemberData(nameof(GetTestCases))]
        public void D_PublishAfterTFMChange(string filename)
        {
            ChangeTargetFramework(filename);
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

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"publish \"{projectPath}\" -c Release -o \"{publishPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                return;
            }
            // Read the output
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();
            Console.WriteLine(output);
            Assert.Equal(0, process.ExitCode);
            Assert.Empty(error);
        }

        //Run the projects created with different TFMs after changing the to latest TFM
        [Theory]
        [MemberData(nameof(GetTestCases))]
        public void E_LaunchAfterTFMChange(string filename)
        {
            ChangeTargetFramework(filename);
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

            Assert.False(process.HasExited);
            // Optionally, you can close the application after the test
            process.Kill();
        }

        //Change the TFM to latest version of .Net
        static void ChangeTargetFramework(string WpfProjectPath)
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

        //Get the list of project path for the created versions.
        public static IEnumerable<object[]> GetTestCases()
        {
            if (s_wpfProjectDirectory != null)
            {
                foreach (var filename in Directory.GetFiles(s_wpfProjectDirectory, "*.csproj", SearchOption.AllDirectories))
                {
                    yield return new object[] { filename };
                }
            }

        }
    }
}
