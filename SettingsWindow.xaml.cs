using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;

namespace ImageCaption
{
    public partial class SettingsWindow : Window
    {
        private AIImageAnalyzer analyzer;
        private AppSettings settings;

        public bool SettingsChanged { get; private set; } = false;

        public SettingsWindow(AppSettings currentSettings)
        {
            InitializeComponent();
            settings = currentSettings.Clone();
            analyzer = new AIImageAnalyzer(settings);
            
            LoadSettings();
            SetupEventHandlers();
        }

        private void LoadSettings()
        {
            ServerUrlTextBox.Text = settings.ServerUrl;
            ModelComboBox.Text = settings.ModelName;
            PromptTextBox.Text = settings.Prompt;
            TimeoutSlider.Value = settings.TimeoutSeconds;
            ConcurrencySlider.Value = settings.ConcurrencyLevel;
            SaveResultsCheckBox.IsChecked = settings.SaveResults;
            SaveFormatComboBox.SelectedIndex = (int)settings.SaveFormat;
            IncludeTimestampCheckBox.IsChecked = settings.IncludeTimestamp;
            
            UpdateTimeoutDisplay();
            UpdateConcurrencyDisplay();
        }

        private void SetupEventHandlers()
        {
            TimeoutSlider.ValueChanged += (s, e) => UpdateTimeoutDisplay();
            ConcurrencySlider.ValueChanged += (s, e) => UpdateConcurrencyDisplay();
        }

        private void UpdateTimeoutDisplay()
        {
            TimeoutValueTextBlock.Text = $"{(int)TimeoutSlider.Value}秒";
        }

        private void UpdateConcurrencyDisplay()
        {
            ConcurrencyValueTextBlock.Text = $"{(int)ConcurrencySlider.Value}";
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            TestConnectionButton.IsEnabled = false;
            ConnectionStatusTextBlock.Text = "接続中...";
            
            try
            {
                var tempSettings = new AppSettings { ServerUrl = ServerUrlTextBox.Text };
                var tempAnalyzer = new AIImageAnalyzer(tempSettings);
                
                var isAvailable = await tempAnalyzer.IsOllamaAvailableAsync();
                
                if (isAvailable)
                {
                    ConnectionStatusTextBlock.Text = "接続成功";
                    await RefreshModels();
                }
                else
                {
                    ConnectionStatusTextBlock.Text = "接続失敗";
                }
            }
            catch (Exception ex)
            {
                ConnectionStatusTextBlock.Text = $"エラー: {ex.Message}";
            }
            finally
            {
                TestConnectionButton.IsEnabled = true;
            }
        }

        private async void RefreshModelsButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshModels();
        }

        private async Task RefreshModels()
        {
            RefreshModelsButton.IsEnabled = false;
            
            try
            {
                var tempSettings = new AppSettings { ServerUrl = ServerUrlTextBox.Text };
                var tempAnalyzer = new AIImageAnalyzer(tempSettings);
                
                var models = await tempAnalyzer.GetAvailableModelsAsync();
                
                AvailableModelsListBox.Items.Clear();
                ModelComboBox.Items.Clear();
                
                foreach (var model in models)
                {
                    AvailableModelsListBox.Items.Add(model);
                    ModelComboBox.Items.Add(model);
                }
                
                if (models.Any(m => m.Contains("llava")))
                {
                    var llavaModel = models.First(m => m.Contains("llava"));
                    ModelComboBox.SelectedItem = llavaModel;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"モデル一覧の取得に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                RefreshModelsButton.IsEnabled = true;
            }
        }

        private void ResetPromptButton_Click(object sender, RoutedEventArgs e)
        {
            PromptTextBox.Text = AppSettings.DefaultPrompt;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateSettings())
            {
                SaveSettings();
                SettingsChanged = true;
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(ServerUrlTextBox.Text))
            {
                MessageBox.Show("サーバーURLを入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(ModelComboBox.Text))
            {
                MessageBox.Show("モデル名を選択または入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PromptTextBox.Text))
            {
                MessageBox.Show("プロンプトを入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void SaveSettings()
        {
            settings.ServerUrl = ServerUrlTextBox.Text;
            settings.ModelName = ModelComboBox.Text;
            settings.Prompt = PromptTextBox.Text;
            settings.TimeoutSeconds = (int)TimeoutSlider.Value;
            settings.ConcurrencyLevel = (int)ConcurrencySlider.Value;
            settings.SaveResults = SaveResultsCheckBox.IsChecked ?? false;
            settings.SaveFormat = (SaveFormat)SaveFormatComboBox.SelectedIndex;
            settings.IncludeTimestamp = IncludeTimestampCheckBox.IsChecked ?? false;
            
            AppSettings.Current = settings;
            AppSettings.Save();
        }

        public AppSettings GetSettings()
        {
            return settings;
        }
    }
}