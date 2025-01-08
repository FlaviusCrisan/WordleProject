using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Controls;

namespace wordleProject
{
    public partial class MainPage : ContentPage
    {
        // File name constants for word list and player data
        private const string WordListFileName = "words.txt";  
        private const string PlayerDataFileName = "playerdata.txt";  

        //List to store words and variables needed
        private List<string> WordList = new List<string>();    
        private string TargetWord;
        private int CurrentRow = 0;
        private int CurrentCol = 0;
        private string CurrentGuess = string.Empty;
        private string PlayerName;
        private int WinStreak = 0;
        private int GamesPlayed = 0;

        public MainPage()
        {
            try
            {
                InitializeComponent();  
                LoadWordList();  
                TargetWord = GetRandomWord();  
            }
            catch (Exception ex)
            {
                // Handles the initialization errors
                DisplayAlert("Initialization Error", $"An error occurred during initialization: {ex.Message}", "OK");
            }
        }

        // Loads word list from a file
        private async void LoadWordList()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string filePath = Path.Combine(folderPath, WordListFileName);

            try
            {
               // Download the word list if it does not exist
                if (!File.Exists(filePath))
                {
                    await DownloadWordList(filePath);
                }

                // Load words into the WordList variable
                WordList.Clear();
                string[] words = File.ReadAllLines(filePath);
                WordList.AddRange(words);

                // Check if the word list is empty
                if (WordList.Count == 0)
                {
                    throw new Exception("The word list is empty. Please check the file or redownload it.");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error Loading Words", $"An error occurred while loading the word list: {ex.Message}", "OK");
            }
        }

        // Method to download the word list from a URL
        private async System.Threading.Tasks.Task DownloadWordList(string filePath)
        {
            string url = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";
            try
            {
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    // Fetches the word list and save it locally
                    string wordListData = await httpClient.GetStringAsync(url);
                    File.WriteAllText(filePath, wordListData);

                    // Add words to the WordList variable
                    string[] words = wordListData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    WordList.AddRange(words);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Download Error", $"Failed to download the word list: {ex.Message}", "OK");
                throw; 
            }
        }


        // Method to get a random word from the word list
        private string GetRandomWord()
        {
            try
            {
                // Ensure the word list is not empty
                if (WordList.Count == 0)
                {
                    throw new Exception("Word list is empty. Ensure words.txt is loaded correctly.");
                }

                // Select a random word
                var random = new Random();
                return WordList[random.Next(WordList.Count)].ToUpper();
            }
            catch (Exception ex)
            {
                // Handle errors while selecting a random word
                DisplayAlert("Error", $"Unable to select a random word: {ex.Message}", "OK");
                return "ERROR"; 
            }
        }


        // Resets the entire game, grid, and keyboard, and prepares for a new game.
        private void ResetGame()
        {
            // Reset the game grid and keyboard
            ResetGrid();
            ResetKeyboard();

            // Choose a random target word
            TargetWord = GetRandomWord();

            // Reset the player's current row and column
            CurrentRow = 0;
            CurrentCol = 0;

            // Clear any current guesses
            CurrentGuess = string.Empty;
        }

        // Resets the game grid to be empty, clearing out the previous game data
        private void ResetGrid()
        {
            foreach (var child in WordleGrid.Children)
            {
                var frame = child as Frame;
                if (frame != null)
                {
                    var label = frame.Content as Label;
                    if (label != null)
                    {
                        // Clear the text and reset background color
                        label.Text = string.Empty;
                        frame.BackgroundColor = Color.FromArgb("#222");
                    }
                }
            }
        }

        // Resets the keyboard's appearance by setting all buttons to their default color
        private void ResetKeyboard()
        {
            foreach (var child in KeyboardLayout.Children)
            {
                if (child is HorizontalStackLayout row)
                {
                    foreach (var button in row.Children)
                    {
                        if (button is Button btn)
                        {
                            // Reset the background color of each button
                            btn.BackgroundColor = Color.FromRgb(129, 131, 132);
                        }
                    }
                }
            }
        }

        // Handles the login button click event, checking the name input and starting the game
        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // Check if the name entry is valid 
            if (!string.IsNullOrEmpty(NameEntry.Text) && NameEntry.Text.Any(char.IsLetterOrDigit))
            {
                // Set the player's name and load their data
                PlayerName = NameEntry.Text;
                LoadPlayerData();

                // Hide the login screen and show the game screen
                LoginLayout.IsVisible = false;
                GameLayout.IsVisible = true;

                // Show the player's streak and games played information
                FireEmoji.IsVisible = true;
                GearIcon.IsVisible = true;
                StreakLabel.Text = WinStreak.ToString();
                NumberLabel.Text = GamesPlayed.ToString();

                // Create the game grid and the keyboard layout
                CreateWordleGrid();
                CreateKeyboard();
            }
            else
            {
                // Show an error message if the input is invalid
                DisplayAlert("Invalid Input", "Please enter at least one letter or number.", "OK");
            }
        }

        // Loads the player data (win streak and games played) from the local storage
        private void LoadPlayerData()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string filePath = folderPath + "/" + PlayerDataFileName;

            if (File.Exists(filePath))
            {
                var data = File.ReadAllLines(filePath);
                if (data.Length == 2)
                {
                    // Load the win streak and games played values from the file
                    WinStreak = int.Parse(data[0]);
                    GamesPlayed = int.Parse(data[1]);
                }
            }
        }

        // Handles the settings (gear) icon click to toggle the visibility of the save button
        private void OnGearIconClicked(object sender, EventArgs e)
        {
            // Toggle visibility of the save button
            SaveButton.IsVisible = !SaveButton.IsVisible;
        }

        // Saves the current player data (win streak and games played) to local storage
        private void OnSaveButtonClicked(object sender, EventArgs e)
        {
            // Save the data and display a confirmation message
            SavePlayerData();
            DisplayAlert("Game Saved", "Your game progress has been saved!", "OK");
        }

        // Saves the player's data to a local file
        private void SavePlayerData()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string filePath = folderPath + "/" + PlayerDataFileName;

            // Save win streak and games played values as a string array
            string[] data = { WinStreak.ToString(), GamesPlayed.ToString() };
            File.WriteAllLines(filePath, data);
        }

        // Creates the game grid layout for Wordle, adding frames for each letter cell
        private void CreateWordleGrid()
        {
            // Clear the existing grid
            WordleGrid.Children.Clear();

            if (WordList.Count == 0)
            {
                // If the word list is empty, show an error
                DisplayAlert("Error", "Word list is empty. Unable to create grid.", "OK");
                return;
            }

            // Create the grid with 6 rows and 5 columns for the letters
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    var frame = new Frame
                    {
                        BackgroundColor = Color.FromArgb("#222"),
                        BorderColor = Color.FromArgb("#555"),
                        WidthRequest = 75,
                        HeightRequest = 75,
                        CornerRadius = 4,
                        Padding = 0,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Content = new Label
                        {
                            Text = string.Empty,
                            TextColor = Colors.White,
                            FontSize = 20,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center
                        }
                    };

                    // Add the frame to the grid
                    WordleGrid.Add(frame, col, row);
                }
            }
        }

        // Creates the  keyboard with letters arranged in rows
        private void CreateKeyboard()
        {
            string[] keyboardRows = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };

            // Clear the existing keyboard layout
            KeyboardLayout.Children.Clear();

            // Add each row of keys to the keyboard layout
            foreach (var row in keyboardRows)
            {
                var rowLayout = new HorizontalStackLayout { Spacing = 6.5, HorizontalOptions = LayoutOptions.Center };
                foreach (var key in row)
                {
                    var button = CreateKeyboardButton(key.ToString());
                    rowLayout.Children.Add(button);
                }
                KeyboardLayout.Children.Add(rowLayout);
            }

            // Add special buttons (like Enter, Backspace)
            AddSpecialKeyboardButtons();
        }


        // Method to create a button for the keyboard keys
        private Button CreateKeyboardButton(string key)
        {
            var button = new Button
            {
                Text = key,
                WidthRequest = 35,
                HeightRequest = 65,
                BackgroundColor = Color.FromRgb(129, 131, 132),
                TextColor = Colors.White,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 5
            };
            button.Clicked += OnKeyboardButtonClicked;
            return button;
        }

        // Method to add special keys (Enter and Remove) to the keyboard
        private void AddSpecialKeyboardButtons()
        {
            var enterButton = new Button
            {
                Text = "ENTER",
                WidthRequest = 70,
                HeightRequest = 65,
                BackgroundColor = Color.FromRgb(129, 131, 132),
                TextColor = Colors.White,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 5
            };
            enterButton.Clicked += OnEnterButtonClicked;

            var removeButton = new Button
            {
                Text = "REMOVE",
                WidthRequest = 85,
                HeightRequest = 65,
                BackgroundColor = Color.FromRgb(129, 131, 132),
                TextColor = Colors.White,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                CornerRadius = 5
            };
            removeButton.Clicked += OnRemoveButtonClicked;

            var lastRow = KeyboardLayout.Children.LastOrDefault() as HorizontalStackLayout;
            if (lastRow != null)
            {
                lastRow.Children.Insert(0, enterButton);
                lastRow.Children.Add(removeButton);
            }
        }

        // Event handler for keyboard key button clicks
        private void OnKeyboardButtonClicked(object sender, EventArgs e)
        {
            if (sender is Button button && CurrentCol < 5)
            {
                string letter = button.Text;
                CurrentGuess += letter;
                var frame = WordleGrid.Children[CurrentRow * 5 + CurrentCol] as Frame;
                if (frame != null)
                {
                    var label = frame.Content as Label;
                    if (label != null)
                    {
                        label.Text = letter;
                    }
                }
                CurrentCol++;
            }
        }

        // Event handler for the Remove button click
        private void OnRemoveButtonClicked(object sender, EventArgs e)
        {
            if (CurrentCol > 0)
            {
                CurrentCol--;
                CurrentGuess = CurrentGuess.Remove(CurrentGuess.Length - 1);
                var frame = WordleGrid.Children[CurrentRow * 5 + CurrentCol] as Frame;
                if (frame != null)
                {
                    var label = frame.Content as Label;
                    if (label != null)
                    {
                        label.Text = string.Empty;
                    }
                }
            }
        }

        // Event handler for the Enter button click
        private void OnEnterButtonClicked(object sender, EventArgs e)
        {
            if (CurrentGuess.Length == 5)
            {
                CheckGuess();
            }
            else
            {
                DisplayAlert("Error", "Your guess must be exactly 5 letters long.", "OK");
            }
        }

        // Method to check the player's guess against the target word
        private void CheckGuess()
        {
            for (int i = 0; i < 5; i++)
            {
                var label = WordleGrid.Children[CurrentRow * 5 + i] as Frame;
                var guessedLetter = CurrentGuess[i];

                // Set background color based on letter correctness
                if (guessedLetter == TargetWord[i])
                    label.BackgroundColor = Color.FromArgb("#6aaa64");
                else if (TargetWord.Contains(guessedLetter))
                    label.BackgroundColor = Color.FromArgb("#c9b458");
                else
                    label.BackgroundColor = Color.FromArgb("#787c7e");
            }

            if (CurrentGuess == TargetWord)
            {
                DisplayAlert("Congratulations!", "You guessed the word!", "OK");
                ResetGame();
                return;
            }

            if (CurrentRow == 5)
            {
                DisplayAlert("Game Over", $"The correct word was {TargetWord}.", "OK");
                ResetGame();
                return;
            }

            CurrentRow++;
            CurrentCol = 0;
            CurrentGuess = string.Empty;
        }
    }
}
