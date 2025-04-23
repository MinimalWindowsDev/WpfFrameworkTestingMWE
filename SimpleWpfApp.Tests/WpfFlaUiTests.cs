using System;
using System.IO;
using System.Reflection;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using Xunit;

namespace SimpleWpfApp.Tests
{
    public class WpfFlaUiTests : IDisposable
    {
        private Application _application;
        private AutomationBase _automation;
        private Window _mainWindow;

        // Helper to find the path to the WPF app executable
        // Helper to find the path to the WPF app executable
        private static string FindAppPath()
        {
            // Use AppContext.BaseDirectory which is generally more reliable for test runners
            // It often points to the original build output directory of the test assembly
            string baseDirectory = AppContext.BaseDirectory;
            Console.WriteLine($"Test Base Directory (AppContext.BaseDirectory): {baseDirectory}"); // Log for debugging

            string configuration = "Debug"; // Default
#if !DEBUG
        configuration = "Release";
#endif

            // Construct the relative path from the *test project's output dir*
            // to the *app project's output dir*.
            // Assumes standard Solution/Project/bin/Config structure:
            // From: <SolutionDir>/SimpleWpfApp.Tests/bin/Debug/
            // To:   <SolutionDir>/SimpleWpfApp/bin/Debug/
            // Relative: ..\..\..\SimpleWpfApp\bin\Debug\SimpleWpfApp.exe

            // We go up three levels from the test output dir (e.g., bin/Debug/) to reach the solution root,
            // then down into the app's output path.
            string appRelativePath = Path.Combine(@"..\..\..\", "SimpleWpfApp", "bin", configuration, "SimpleWpfApp.exe");
            string potentialAppPath = Path.Combine(baseDirectory, appRelativePath);

            // Resolve the relative path parts (..) to get a clean absolute path
            string fullAppPath = Path.GetFullPath(potentialAppPath);

            Console.WriteLine($"Calculated App Path: {fullAppPath}"); // Log for debugging

            if (!File.Exists(fullAppPath))
            {
                // Provide a more detailed error message
                throw new FileNotFoundException(
                    $"SimpleWpfApp.exe not found. \n" +
                    $"Checked path: {fullAppPath}\n" +
                    $"This path was calculated relative to the test execution base directory: {baseDirectory}\n" +
                    $"Configuration used: {configuration}\n" +
                    $"Troubleshooting:\n" +
                    $"1. Ensure the 'SimpleWpfApp' project is built successfully in the '{configuration}' configuration.\n" +
                    $"2. Verify the project structure matches the assumed relative path (Solution -> SimpleWpfApp.Tests & SimpleWpfApp).\n" +
                    $"3. Check the actual build output path for SimpleWpfApp.exe.");
            }

            Console.WriteLine($"Confirmed App Exists At: {fullAppPath}");
            return fullAppPath;
        }


        public WpfFlaUiTests()
        {
            try
            {
                string appPath = FindAppPath();
                _application = Application.Launch(appPath);
                _automation = new UIA3Automation();

                // Give the app a moment to start - adjust time if needed
                // In real tests, use more robust waiting (e.g., Retry)
                _application.WaitWhileMainHandleIsMissing(TimeSpan.FromSeconds(5));

                _mainWindow = _application.GetMainWindow(_automation);

                if (_mainWindow == null)
                {
                    throw new InvalidOperationException($"Could not find main window for '{appPath}'. App might have crashed on startup.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during test setup: {ex.Message}\n{ex.StackTrace}");
                // Clean up partially started resources if setup fails
                Dispose();
                throw; // Re-throw to fail the test immediately
            }
        }

        [Fact]
        public void GreetButton_ClickWithName_ShowsCorrectGreeting()
        {
            // Arrange: Find UI elements using AutomationId
            var nameTextBox = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NameTextBoxId"))?.AsTextBox();
            var greetButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("GreetButtonId"))?.AsButton();
            var greetingTextBlock = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("GreetingTextBlockId")); // Get the base AutomationElement

            Assert.NotNull(nameTextBox); // Ensure elements were found
            Assert.NotNull(greetButton);
            Assert.NotNull(greetingTextBlock);

            // Act: Interact with the UI
            nameTextBox.Text = "FlaUI User";
            Thread.Sleep(100); // Small pause can sometimes help stability
            greetButton.Click();
            Thread.Sleep(500); // Wait for UI update (adjust if needed)

            // Assert: Check the result in the TextBlock
            // For TextBlock, the content is often in the 'Name' property for UIA
            Assert.Equal("Hello, FlaUI User!", greetingTextBlock.Name);
        }

        [Fact]
        public void GreetButton_ClickWithEmptyName_ShowsPrompt()
        {
            // Arrange
            var nameTextBox = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NameTextBoxId")).AsTextBox();
            var greetButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("GreetButtonId")).AsButton();
            var greetingTextBlock = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("GreetingTextBlockId")); // Get the base AutomationElement

            Assert.NotNull(nameTextBox);
            Assert.NotNull(greetButton);
            Assert.NotNull(greetingTextBlock);

            // Act
            nameTextBox.Text = ""; // Ensure it's empty
            Thread.Sleep(100);
            greetButton.Click();
            Thread.Sleep(500);

            // Assert
            Assert.Equal("Please enter your name.", greetingTextBlock.Name);
        }

        // Cleanup: Ensures the WPF app is closed after tests run
        public void Dispose()
        {
            _mainWindow?.Close(); // Try to close gracefully
            _automation?.Dispose();
            if (_application != null && !_application.HasExited)
            {
                Console.WriteLine($"Application Process {_application.ProcessId} did not exit, killing.");
                _application.Kill(); // Force kill if not closed
            }
            _application?.Dispose();
        }
    }
}