namespace wordleProject
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // Check if the entry has one or more letter/number
            if (!string.IsNullOrEmpty(NameEntry.Text) && NameEntry.Text.Any(char.IsLetterOrDigit))
            {
                // Hide the Wordle label and login layout
                WordleLabel.IsVisible = false;
                LoginLayout.IsVisible = false;
            }
            else
            {
                // Display an alert if the input is invalid
                DisplayAlert("Invalid Input", "Please enter at least one letter or number.", "OK");
            }
        }
    }
}
