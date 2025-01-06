using System;
using System.Collections.Generic;
using System.Linq;


// Do comments at end tomorrow + integrate proper word list + file saving & loading + android support + extras + delete this comment when done 
namespace wordleProject
{
    public partial class MainPage : ContentPage
    {
        private List<string> WordList = new List<string>
        {
            "apple", "plane", "zebra", "crane", "eagle", "table", "chair", "blame", "grape", "house",
            "snake", "flame", "white", "quick", "liver", "tiger", "light", "shark", "grind", "float"
        };

        private string TargetWord;
        private int CurrentRow = 0;
        private int CurrentCol = 0;
        private string CurrentGuess = string.Empty;

        public MainPage()
        {
            InitializeComponent();
            TargetWord = GetRandomWord();
        }

        private string GetRandomWord()
        {
            var random = new Random();
            return WordList[random.Next(WordList.Count)].ToUpper();
        }

        private void ResetGame()
        {
            ResetGrid();
            ResetKeyboard();
            TargetWord = GetRandomWord();
            CurrentRow = 0;
            CurrentCol = 0;
            CurrentGuess = string.Empty;
        }

        private void ResetGrid()
        {
            foreach (var child in WordleGrid.Children.OfType<Frame>())
            {
                var label = child.Content as Label;
                if (label != null)
                {
                    label.Text = string.Empty;
                    child.BackgroundColor = Color.FromArgb("#222");
                }
            }
        }

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
                            btn.BackgroundColor = Color.FromRgb(129, 131, 132);
                        }
                    }
                }
            }
        }


        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // Check if the entry has one or more letter/number
            if (!string.IsNullOrEmpty(NameEntry.Text) && NameEntry.Text.Any(char.IsLetterOrDigit))
            {
                // Hide the Wordle label and login layout
                LoginLayout.IsVisible = false;
                GameLayout.IsVisible = true;

                FireEmoji.IsVisible = true;
                GearIcon.IsVisible = true;

                CreateWordleGrid();
                CreateKeyboard();
            }
            else
            {
                // Display an alert if the input is invalid
                DisplayAlert("Invalid Input", "Please enter at least one letter or number.", "OK");
            }
        }

        private void CreateWordleGrid()
        {
            WordleGrid.Children.Clear();

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

                    WordleGrid.Add(frame, col, row);
                }
            }
        }

        private void CreateKeyboard()
        {
            string[] keyboardRows = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };

            KeyboardLayout.Children.Clear();

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

            AddSpecialKeyboardButtons();
        }

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

        private void CheckGuess()
        {
            for (int i = 0; i < 5; i++)
            {
                var label = WordleGrid.Children[CurrentRow * 5 + i] as Frame;
                var guessedLetter = CurrentGuess[i];

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
