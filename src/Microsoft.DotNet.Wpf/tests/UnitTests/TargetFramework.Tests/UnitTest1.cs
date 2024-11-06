using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace TargetFramework.Tests
{
    public class UnitTest1
    {
        private const string WpfProjectDirectory = "D:\\WPF\\wpf\\artifacts\\bin\\WPFProjectGenerator\\Debug\\net9.0TFMProjects";
        //Build the project with default framework
        [Test, TestCaseSource(nameof(GetTestCases))]
        public void Test_BuildAllTFMs(string filename)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{filename}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start the process.");
                return;
            }
            // Read the output
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            Assert.Multiple(() =>
            {
                // Assert the build was successful (exit code 0)

                Assert.That(process.ExitCode, Is.EqualTo(0));
                Assert.That(error, Is.Empty); // Optionally check for any build errors
            });
        }

        //Launch the project with default framework
        [Test, TestCaseSource(nameof(GetExeToLaunch))]
        public void Test1_LaunchAllTFMs(string filename)
        {
            // Start the WPF application
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    // FileName = @"C:\Users\v-vmb\source\repos\WPFTFMtest\WPFProjectGenerator\WPFProjectGenerator\bin\Debug\net9.0TFMProjects\TestSampleWPFNET8\TestSampleWPFNET8\bin\Debug\net8.0-windows\TestSampleWPFNET8.exe",
                    UseShellExecute = true,
                }
            };

            process.Start();

            // Optionally wait for a moment to ensure the app is up
            Thread.Sleep(2000);

            // Assert that the process is running
            //  Assert.True(!process.HasExited, "WPF application did not start.");
            Assert.That(!process.HasExited, Is.True);

            // Optionally, you can close the application after the test
            process.Kill();
        }

        //Build the project with latest framework
        [Test, TestCaseSource(nameof(GetTestCases))]
        public void Test2_BuildAfterTFMChange(string filename)
        {
            // Define the path to your WPF project file
            //var projectPath = "../PathToYourWpfProject/YourWpfProject.csproj";
            ChangeTargetFramework(filename);
            // Create the process to run the dotnet build command
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{filename}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start the process.");
                return;
            }
            // Read the output
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            Assert.Multiple(() =>
            {
                // Assert the build was successful (exit code 0)
                Assert.That(process.ExitCode, Is.EqualTo(0));
                Assert.That(error, Is.Empty);// Optionally check for any build errors
            });
        }

        //Launch the project with latest framework
        [Test, TestCaseSource(nameof(GetExeToLaunch))]
        public void Test3_LaunchAfterTFMChange(string filename)
        {
            // Start the WPF application
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    UseShellExecute = true,
                }
            };

            process.Start();

            // Optionally wait for a moment to ensure the app is up
            Thread.Sleep(2000);

            // Assert that the process is running
            //Assert.True(!process.HasExited, "WPF application did not start.");
            Assert.That(!process.HasExited, Is.True);
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
            var propertyGroup = doc.Descendants("PropertyGroup").FirstOrDefault();

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

        private static string[] GetTestCases()
        {

            string[] files = System.IO.Directory.GetFiles(WpfProjectDirectory, "*.csproj", SearchOption.AllDirectories);
            return files;
        }

        private static string[] GetExeToLaunch()
        {
            string[] files = System.IO.Directory.GetFiles(WpfProjectDirectory, "*.exe", SearchOption.AllDirectories);
            return files;
        }
    }
}
