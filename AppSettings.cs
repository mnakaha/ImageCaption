using System;
using System.IO;
using Newtonsoft.Json;

namespace ImageCaption
{
    public enum SaveFormat
    {
        JSON = 0,
        CSV = 1,
        TXT = 2
    }

    public class AppSettings
    {
        public const string DefaultPrompt = "この画像を詳しく日本語で説明してください。画像に含まれる物体、人物、場所、色、雰囲気などを具体的に説明してください。";
        
        private static AppSettings _current;
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "ImageCaption", 
            "settings.json");

        public string ServerUrl { get; set; } = "http://localhost:11434";
        public string ModelName { get; set; } = "llava";
        public string Prompt { get; set; } = DefaultPrompt;
        public int TimeoutSeconds { get; set; } = 300;
        public int ConcurrencyLevel { get; set; } = 1;
        public bool SaveResults { get; set; } = false;
        public SaveFormat SaveFormat { get; set; } = SaveFormat.JSON;
        public bool IncludeTimestamp { get; set; } = true;

        public static AppSettings Current
        {
            get
            {
                if (_current == null)
                {
                    _current = Load();
                }
                return _current;
            }
            set
            {
                _current = value;
            }
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                    return settings ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定読み込みエラー: {ex.Message}");
            }

            return new AppSettings();
        }

        public static void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(Current, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定保存エラー: {ex.Message}");
            }
        }

        public AppSettings Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<AppSettings>(json);
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ServerUrl))
                ServerUrl = "http://localhost:11434";
            
            if (string.IsNullOrWhiteSpace(ModelName))
                ModelName = "llava";
            
            if (string.IsNullOrWhiteSpace(Prompt))
                Prompt = DefaultPrompt;
            
            if (TimeoutSeconds < 30)
                TimeoutSeconds = 30;
            else if (TimeoutSeconds > 600)
                TimeoutSeconds = 600;
            
            if (ConcurrencyLevel < 1)
                ConcurrencyLevel = 1;
            else if (ConcurrencyLevel > 5)
                ConcurrencyLevel = 5;
        }
    }
}