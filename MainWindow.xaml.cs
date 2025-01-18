using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;

namespace FiveLetters
{
    public partial class MainWindow : Window
    {
        private string targetWord; // Загаданное слово
        private int currentRow = 0; // Текущая строка для ввода слова
        private bool isGameOver = false; // Флаг завершения игры

        private List<string> words; // Список слов

        public MainWindow()
        {
            InitializeComponent();
            LoadWordsFromFile("words.txt");
            StartNewGame();
        }

        private void LoadWordsFromFile(string filePath)
        {
            words = new List<string>();
            try
            {
                foreach (var line in File.ReadLines(filePath))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        words.Add(line.Trim().ToUpper());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке слов из файла: {ex.Message}");
            }
        }

        private void StartNewGame()
        {
            if (words.Count == 0)
            {
                MessageBox.Show("Список слов пуст");
                return;
            }

            Random random = new Random();
            targetWord = words[random.Next(words.Count)];
            ResetGame();
            ResetKeyboardButtons();
        }

        private void ResetGame()
        {
            currentRow = 0;
            isGameOver = false;

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    var cell = FindName($"Cell{row}{col}") as Border;
                    if (cell != null)
                    {
                        cell.Background = Brushes.Black;
                        cell.Child = null;
                    }
                }
            }

            NextButton.IsEnabled = false;
            CheckButton.IsEnabled = true;
            EraseButton.IsEnabled = true;
            ResetKeyboardButtons();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (isGameOver)
            {
                return;
            }

            var button = sender as Button;
            if (button != null)
            {
                var cell = FindCell(currentRow);
                if (cell != null && cell.Child == null)
                {
                    TextBlock textBlock = new TextBlock
                    {
                        Text = button.Content.ToString(),
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 24
                    };
                    cell.Child = textBlock;
                }
            }
        }

        private Border FindCell(int row)
        {
            for (int col = 0; col < 5; col++)
            {
                var cell = FindName($"Cell{row}{col}") as Border;
                if (cell != null && cell.Child == null)
                {
                    return cell;
                }
            }
            return null;
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            if (isGameOver)
            {
                return;
            }

            string guessedWord = "";
            for (int col = 0; col < 5; col++)
            {
                var cell = FindName($"Cell{currentRow}{col}") as Border;
                if (cell?.Child is TextBlock textBlock)
                {
                    guessedWord += textBlock.Text;
                }
            }

            if (guessedWord.Length == 5)
            {
                if (guessedWord == targetWord)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        var cell = FindName($"Cell{currentRow}{col}") as Border;
                        if (cell != null)
                        {
                            cell.Background = Brushes.Violet;
                        }
                    }
                    NextButton.IsEnabled = true;
                    isGameOver = true;
                    CheckButton.IsEnabled = false;
                    AnewButton.IsEnabled = false;
                    EraseButton.IsEnabled = false;
                }
                else
                {
                    for (int col = 0; col < 5; col++)
                    {
                        var cell = FindName($"Cell{currentRow}{col}") as Border;
                        if (cell?.Child is TextBlock textBlock)
                        {
                            char guessedChar = textBlock.Text[0];
                            if (targetWord.Contains(guessedChar))
                            {
                                if (targetWord[col] == guessedChar)
                                {
                                    cell.Background = Brushes.Violet;
                                }
                                else
                                {
                                    cell.Background = Brushes.LightGray;
                                }
                            }
                            UpdateKeyboardButtonColor(guessedChar, cell.Background);
                        }
                    }
                    if (currentRow == 5)
                    {
                        AnewButton.IsEnabled = true;
                        CheckButton.IsEnabled = false;
                        EraseButton.IsEnabled = false;
                        isGameOver = true;
                    }
                    else
                    {
                        currentRow++;
                    }
                }
            }
        }

        private void UpdateKeyboardButtonColor(char letter, Brush color)
        {
            foreach (var child in KeyboardGrid.Children)
            {
                if (child is Button button && button.Content.ToString() == letter.ToString())
                {
                    if (button.Background != Brushes.Violet)
                    {
                        button.Background = color;
                    }
                    break;
                }
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
        }

        private void DeleteCharButton_Click(object sender, RoutedEventArgs e)
        {
            if (isGameOver)
            {
                return;
            }

            for (int col = 4; col >= 0; col--)
            {
                var cell = FindName($"Cell{currentRow}{col}") as Border;
                if (cell?.Child != null)
                {
                    cell.Child = null;
                    break;
                }
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void ResetKeyboardButtons()
        {
            foreach (var child in KeyboardGrid.Children)
            {
                if (child is Button button)
                {
                    button.Background = Brushes.Black;
                }
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show
                (
                    "Буква на своем месте - фиолетовая подсветка\n" +
                    "Буква не на своем месте - серая подсветка\n" +
                    "Кнопка <Заново> обнулит игру, сохранив слово(доступно после 6 неудачных попыток)\n" +
                    "Кнопка <Стереть> удалит последнюю букву\n" +
                    "Кнопка <Дальше> загадает новое слово(доступно после правильного ответа)\n" +
                    "Кнопка <Проверить> проверит наличие введенных букв в загаданном слове\n",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
        }
    }
}