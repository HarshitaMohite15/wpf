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
        //private const string WpfProjectDirectory = "C:\\Users\\v-vmb\\source\\repos\\TFMWpfRelease9\\artifacts\\bin\\TargetFramework.Tests\\Debug\\net9.0TFMProjects";
        //private string appPath = "C:\\Users\\v-vmb\\source\\repos\\TFMWpfRelease9\\artifacts\\bin\\WPFProjectGenerator\\Debug\\net9.0\\WPFProjectGenerator.exe";

        private readonly IConfiguration _configuration;
        private static string? s_wpfProjectDirectory;
        readonly string? _appPath;
        public XUnitTest(ConfigurationFixture configFixture)
        {
            _configuration = configFixture.Configuration;
            s_wpfProjectDirectory = _configuration["WpfProjectDirectory"];
            _appPath = _configuration["appPath"];
        }
        
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

        static void ChangeTargetFramework(string WpfProjectPath)
        {
            string newTargetFramework = "net9.0-windows";
            if (!File.Exists(WpfProjectPath))
            {
                Console.WriteLine("Project file not found.");
                return;
            }

            // Load the project file
            XDocument doc = XDocument.Load(WpfProjectPath);
            XElement? propertyGroup = doc.Descendants("PropertyGroup").FirstOrDefault();

            if (propertyGroup != null)
            {
                var targetFrameworkElement = propertyGroup.Elements("TargetFramework").FirstOrDefault();
                if (targetFrameworkElement != null)
                {
                    targetFrameworkElement.Value = newTargetFramework;
                }
                else
                {
                    propertyGroup.Add(new XElement("TargetFramework", newTargetFramework));
                }

                // Save the changes
                doc.Save(WpfProjectPath);
                Console.WriteLine($"Target framework changed to '{newTargetFramework}' in '{WpfProjectPath}'.");
            }
            else
            {
                Console.WriteLine("No PropertyGroup found in the project file.");
            }
        }
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
