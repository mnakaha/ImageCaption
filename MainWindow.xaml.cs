using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace ImageCaption
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ImageInfo> imageInfos = new ObservableCollection<ImageInfo>();
        private readonly string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif" };
        private AIImageAnalyzer analyzer;

        public MainWindow()
        {
            InitializeComponent();
            ImageListBox.ItemsSource = imageInfos;
            analyzer = new AIImageAnalyzer(AppSettings.Current);
            CheckOllamaStatus();
        }

        private async void CheckOllamaStatus()
        {
            var isAvailable = await analyzer.IsOllamaAvailableAsync();
            if (!isAvailable)
            {
                StatusTextBlock.Text = "Ollamaサーバーに接続できません。Ollamaが起動していることを確認してください。";
                AnalyzeButton.IsEnabled = false;
            }
            else
            {
                StatusTextBlock.Text = "Ollama接続確認済み - 準備完了";
                var models = await analyzer.GetAvailableModelsAsync();
                if (!models.Any(m => m.Contains("llava")))
                {
                    StatusTextBlock.Text = "LLaVAモデルが見つかりません。'ollama pull llava'でモデルをインストールしてください。";
                }
            }
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "画像ファイルが含まれるフォルダを選択してください";
                folderDialog.ShowNewFolderButton = false;

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderPathTextBox.Text = folderDialog.SelectedPath;
                    AnalyzeButton.IsEnabled = true;
                    imageInfos.Clear();
                }
            }
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = FolderPathTextBox.Text;
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                MessageBox.Show("有効なフォルダを選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AnalyzeButton.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            StatusTextBlock.Text = "画像ファイルを検索中...";

            try
            {
                await AnalyzeImagesInFolder(folderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                AnalyzeButton.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
                StatusTextBlock.Text = $"解析完了: {imageInfos.Count}枚の画像を処理しました。";
            }
        }

        private async Task AnalyzeImagesInFolder(string folderPath)
        {
            var imageFiles = GetImageFiles(folderPath);
            ProgressBar.Maximum = imageFiles.Count;
            ProgressBar.Value = 0;

            foreach (var imageFile in imageFiles)
            {
                StatusTextBlock.Text = $"解析中: {Path.GetFileName(imageFile)}";
                
                try
                {
                    var imageInfo = new ImageInfo
                    {
                        FilePath = imageFile,
                        FileName = Path.GetFileName(imageFile),
                        ThumbnailPath = imageFile
                    };

                    imageInfo.Caption = await analyzer.AnalyzeImageAsync(imageFile);
                    imageInfos.Add(imageInfo);
                }
                catch (Exception ex)
                {
                    var errorImageInfo = new ImageInfo
                    {
                        FilePath = imageFile,
                        FileName = Path.GetFileName(imageFile),
                        ThumbnailPath = imageFile,
                        Caption = $"エラー: {ex.Message}"
                    };
                    imageInfos.Add(errorImageInfo);
                }

                ProgressBar.Value++;
            }
        }

        private List<string> GetImageFiles(string folderPath)
        {
            var imageFiles = new List<string>();
            
            try
            {
                foreach (var extension in supportedExtensions)
                {
                    imageFiles.AddRange(Directory.GetFiles(folderPath, $"*{extension}", SearchOption.AllDirectories));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"フォルダの読み込みでエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return imageFiles;
        }

        private void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageListBox.SelectedItem is ImageInfo selectedImage)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedImage.FilePath);
                    bitmap.DecodePixelWidth = 280;
                    bitmap.EndInit();
                    PreviewImage.Source = bitmap;
                    CaptionTextBox.Text = selectedImage.Caption;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"画像の読み込みでエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(AppSettings.Current);
            if (settingsWindow.ShowDialog() == true)
            {
                if (settingsWindow.SettingsChanged)
                {
                    analyzer = new AIImageAnalyzer(AppSettings.Current);
                    CheckOllamaStatus();
                    StatusTextBlock.Text = "設定が更新されました。";
                }
            }
        }
    }

    public class ImageInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
    }
}