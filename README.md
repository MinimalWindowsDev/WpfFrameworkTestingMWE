# WPF .NET Framework Testing MWE (xUnit, FlaUI, Coverage)

[![License: WTFPL](https://img.shields.io/badge/License-WTFPL-brightgreen.svg)](http://www.wtfpl.net/about/)

A Minimal Working Example (MWE) demonstrating unit testing, UI automation testing, and code coverage for a simple WPF application built with .NET Framework 4.8, using Visual Studio 2019 Professional.

This project showcases:
*   A basic WPF application (`SimpleWpfApp`).
*   Unit-style testing of WPF window logic using xUnit (handling STA thread requirements).
*   UI automation testing using FlaUI and xUnit.
*   Code coverage integration using the Fine Code Coverage extension for Visual Studio.

## Core Technologies

*   **IDE:** Microsoft Visual Studio 2019 Professional
*   **Framework:** .NET Framework 4.8
*   **Language:** C#
*   **UI:** Windows Presentation Foundation (WPF)
*   **Unit Testing Framework:** xUnit
*   **UI Automation Library:** FlaUI (UIA3 wrapper)
*   **Code Coverage Tool:** Fine Code Coverage (Visual Studio Extension)

## Prerequisites

*   **Microsoft Visual Studio 2019 Professional** (or higher supporting .NET Framework 4.8 development).
*   **.NET Framework 4.8 Developer Pack** (usually installed with Visual Studio).
*   **Fine Code Coverage Extension:** Install this from `Extensions` -> `Manage Extensions` in Visual Studio.
*   **Git** (for cloning the repository).

## Getting Started

1.  **Clone the Repository:**
    ```bash
    git clone https://github.com/MinimalWindowsDev/WpfFrameworkTestingMWE
    cd WpfFrameworkTestingMWE
    ```
2.  **Open in Visual Studio:**
    *   Open Visual Studio 2019 Professional.
    *   Select `File` -> `Open` -> `Project/Solution...`.
    *   Navigate to the cloned `WpfFrameworkTestingMWE` folder and open the `WpfFrameworkTestingMWE.sln` file.
    *   Allow Visual Studio to restore NuGet packages if prompted.

## Building the Project

*   Go to `Build` -> `Build Solution` in Visual Studio.
*   Or press `Ctrl+Shift+B`.
*   Ensure the build completes without errors in the Output window.

## Running the Application

1.  In Solution Explorer, right-click on the `SimpleWpfApp` project.
2.  Select `Set as Startup Project`.
3.  Press `F5` or click the "Start" button (with the green play icon) in the toolbar.
4.  The simple WPF application window should appear. You can type in the text box and click the "Greet" button.

## Running the Tests

1.  Go to `Test` -> `Test Explorer` in Visual Studio.
2.  The Test Explorer window will open, discovering tests from the `SimpleWpfApp.Tests` project. You should see tests listed under `UnitTests` and `WpfFlaUiTests`.
3.  Click the `Run All Tests In View` button (double green arrow icon) at the top of Test Explorer.
4.  **Observe:**
    *   The `UnitTests` should run very quickly.
    *   The `WpfFlaUiTests` will launch the `SimpleWpfApp` window briefly, interact with it automatically, and then close it.
5.  All tests should pass (indicated by green checkmarks).

## Viewing Code Coverage

1.  Ensure tests have been run successfully via Test Explorer (as described above).
2.  Go to `View` -> `Other Windows` -> `Fine Code Coverage`.
3.  A "Fine Code Coverage" panel will open (usually docked at the bottom).
4.  **Important:** You may need to **drag the top border of this panel upwards** to make it tall enough to see the results properly.
5.  The panel will analyze the test execution results and display coverage information:
    *   Look for the `SimpleWpfApp` assembly.
    *   Expand it to see coverage for `SimpleWpfApp.MainWindow`.
    *   Clicking on `MainWindow.xaml.cs` will open the file with coverage indicators (green bars for covered lines, red for uncovered).
    *   Note: `SimpleWpfApp.Properties.Settings` showing 0% coverage is expected as the default settings code is auto-generated and not executed by the current app/tests.

## Project Structure

```
WpfFrameworkTestingMWE/
├── .gitignore                 # Specifies intentionally untracked files that Git should ignore.
├── LICENSE                    # The WTFPL license file.
├── README.md                  # This file.
├── WpfFrameworkTestingMWE.sln # Visual Studio Solution file.
├── SimpleWpfApp/              # The main WPF Application project (.NET Framework 4.8)
│   ├── Properties/
│   │   └── AssemblyInfo.cs
│   ├── App.config
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml        # The main window UI definition.
│   ├── MainWindow.xaml.cs     # Code-behind logic for the main window.
│   └── SimpleWpfApp.csproj    # Project file.
└── SimpleWpfApp.Tests/        # The xUnit Test project (.NET Framework 4.8)
    ├── Properties/
    │   └── AssemblyInfo.cs
    ├── packages.config        # Lists NuGet package dependencies.
    ├── UnitTests.cs           # Contains unit-style tests directly interacting with MainWindow.
    ├── WpfFlaUiTests.cs       # Contains UI automation tests using FlaUI.
    └── SimpleWpfApp.Tests.csproj # Test project file.
```

## Testing Approaches Explained

This MWE demonstrates two distinct ways to test the WPF application:

### 1. Unit-Style Tests (`UnitTests.cs`)

*   **Concept:** These tests create an instance of the `MainWindow` class directly and call its public methods (`SetNameAndTriggerUpdate`, `GetGreetingText`) to verify logic.
*   **STA Thread Requirement:** WPF UI elements *must* be created and accessed on a Single-Threaded Apartment (STA) thread. Since xUnit v2 for .NET Framework doesn't run tests on an STA thread by default, a helper method (`RunOnStaThread`) is used to force execution onto an STA thread.
*   **Pros:** Faster execution than UI automation. Tests logic in isolation (mostly).
*   **Cons:** Requires careful STA thread management, can be brittle if UI elements are tightly coupled, doesn't test the actual user interaction flow or XAML bindings triggered by events. Testing event handlers directly is difficult.

```csharp
// Snippet from UnitTests.cs
[Fact]
public void UpdateGreeting_WithName_SetsCorrectText()
{
    MainWindow window = null;
    string greeting = null;
    RunOnStaThread(() => // Force STA thread
    {
        window = new MainWindow();
        window.SetNameAndTriggerUpdate("Test User");
        greeting = window.GetGreetingText();
    });
    Assert.Equal("Hello, Test User!", greeting);
}
```

### 2. UI Automation Tests (`WpfFlaUiTests.cs`)

*   **Concept:** These tests use the FlaUI library (a wrapper around Microsoft UI Automation) to launch the `SimpleWpfApp.exe` as a separate process and interact with its UI elements as a user would (finding buttons/textboxes by `AutomationId`, clicking, typing text, reading results).
*   **End-to-End:** Tests the full flow from UI interaction to the resulting UI update.
*   **Pros:** Tests the application from the user's perspective, verifies UI elements are correctly wired up, doesn't require direct access to the Window's code (treats it as a black box), less concerned with internal threading models.
*   **Cons:** Slower execution, can be sensitive to timing issues (requires waits/sleeps), dependent on stable `AutomationId`s, finding the application executable (`FindAppPath`) requires careful path management. Test setup (`Constructor`) and cleanup (`Dispose`) are critical.

```csharp
// Snippet from WpfFlaUiTests.cs
[Fact]
public void GreetButton_ClickWithName_ShowsCorrectGreeting()
{
    // Arrange: Find UI elements using AutomationId
    var nameTextBox = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("NameTextBoxId"))?.AsTextBox();
    var greetButton = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("GreetButtonId"))?.AsButton();
    var greetingTextBlock = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("GreetingTextBlockId")); // Get AutomationElement

    // Act: Interact with the UI
    nameTextBox.Text = "FlaUI User";
    greetButton.Click();
    Thread.Sleep(500); // Wait for UI update

    // Assert: Check the result in the TextBlock's Name property
    Assert.Equal("Hello, FlaUI User!", greetingTextBlock.Name);
}
```

## License

This project is licensed under the **WTFPL – Do What The Fuck You Want To Public License**. See the [LICENSE](LICENSE) file for details. In short: Do whatever you want.
