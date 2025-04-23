using Xunit;
using SimpleWpfApp; // Add reference to the main app namespace
using System.Threading; // Required for STA thread management

namespace SimpleWpfApp.Tests
{
    public class UnitTests
    {
        // IMPORTANT: WPF UI elements MUST be created and accessed on an STA thread.
        // xUnit v2 for .NET Framework doesn't automatically run tests on STA threads.
        // We use a helper method to force execution onto an STA thread.

        private void RunOnStaThread(ThreadStart testAction)
        {
            Thread thread = new Thread(testAction);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join(); // Wait for the test action to complete
        }

        [Fact]
        public void UpdateGreeting_WithName_SetsCorrectText()
        {
            MainWindow window = null; // Declare outside STA scope
            string greeting = null;

            RunOnStaThread(() =>
            {
                // Arrange
                window = new MainWindow(); // Create window inside STA thread

                // Act
                window.SetNameAndTriggerUpdate("Test User");

                // Assert (get value inside STA thread)
                greeting = window.GetGreetingText();
            });

            // Assert outside STA thread
            Assert.Equal("Hello, Test User!", greeting);
        }

        [Fact]
        public void UpdateGreeting_WithEmptyName_SetsPromptText()
        {
            MainWindow window = null;
            string greeting = null;

            RunOnStaThread(() =>
            {
                // Arrange
                window = new MainWindow();

                // Act
                window.SetNameAndTriggerUpdate("");

                // Assert
                greeting = window.GetGreetingText();
            });

            Assert.Equal("Please enter your name.", greeting);
        }
    }
}