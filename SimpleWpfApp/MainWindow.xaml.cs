using System.Windows;

namespace SimpleWpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GreetButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateGreeting();
        }

        // Public method for potential direct testing (and used by button click)
        public void UpdateGreeting()
        {
            string name = NameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(name))
            {
                GreetingTextBlock.Text = $"Hello, {name}!";
            }
            else
            {
                GreetingTextBlock.Text = "Please enter your name.";
            }
        }

        // Helper method to make *direct* testing slightly easier (optional)
        public string GetGreetingText()
        {
            return GreetingTextBlock.Text;
        }

        // Helper method to make *direct* testing slightly easier (optional)
        public void SetNameAndTriggerUpdate(string name)
        {
            NameTextBox.Text = name;
            UpdateGreeting(); // Call the logic directly
        }
    }
}