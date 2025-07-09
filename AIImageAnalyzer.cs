using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace ImageCaption
{
    public class AIImageAnalyzer
    {
        private readonly HttpClient httpClient;
        private readonly AppSettings settings;

        public AIImageAnalyzer(AppSettings appSettings)
        {
            settings = appSettings;
            settings.Validate();
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        }

        public async Task<string> AnalyzeImageAsync(string imagePath)
        {
            try
            {
                var base64Image = ConvertImageToBase64(imagePath);
                var response = await SendToOllamaAsync(base64Image);
                
                return response ?? "画像の解析に失敗しました。";
            }
            catch (Exception ex)
            {
                return $"エラー: {ex.Message}";
            }
        }

        private async Task<string> SendToOllamaAsync(string base64Image)
        {
            try
            {
                var requestData = new
                {
                    model = settings.ModelName,
                    prompt = settings.Prompt,
                    images = new[] { base64Image },
                    stream = false
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{settings.ServerUrl}/api/generate", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return "Ollamaサーバーに接続できません。Ollamaが起動していることを確認してください。";
                    }
                    
                    return $"API request failed: {response.StatusCode} - {errorContent}";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<OllamaResponse>(responseContent);
                
                return apiResponse?.Response ?? "解析結果を取得できませんでした。";
            }
            catch (HttpRequestException)
            {
                return "Ollamaサーバーに接続できません。Ollamaが起動していることを確認してください。";
            }
            catch (TaskCanceledException)
            {
                return "解析がタイムアウトしました。画像が大きすぎるか、モデルの処理に時間がかかっています。";
            }
            catch (Exception ex)
            {
                return $"予期しないエラーが発生しました: {ex.Message}";
            }
        }

        private string ConvertImageToBase64(string imagePath)
        {
            try
            {
                using (var image = Image.FromFile(imagePath))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        var format = GetImageFormat(imagePath);
                        image.Save(memoryStream, format);
                        var imageBytes = memoryStream.ToArray();
                        return Convert.ToBase64String(imageBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"画像の変換に失敗しました: {ex.Message}");
            }
        }

        private ImageFormat GetImageFormat(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                ".png" => ImageFormat.Png,
                ".gif" => ImageFormat.Gif,
                ".bmp" => ImageFormat.Bmp,
                ".tiff" or ".tif" => ImageFormat.Tiff,
                _ => ImageFormat.Jpeg
            };
        }

        public async Task<bool> IsOllamaAvailableAsync()
        {
            try
            {
                var response = await httpClient.GetAsync($"{settings.ServerUrl}/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetAvailableModelsAsync()
        {
            try
            {
                var response = await httpClient.GetAsync($"{settings.ServerUrl}/api/tags");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<string>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var modelsResponse = JsonConvert.DeserializeObject<ModelsResponse>(responseContent);
                
                var modelNames = new List<string>();
                if (modelsResponse?.Models != null)
                {
                    foreach (var model in modelsResponse.Models)
                    {
                        modelNames.Add(model.Name ?? "Unknown");
                    }
                }
                
                return modelNames;
            }
            catch
            {
                return new List<string>();
            }
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }

    public class OllamaResponse
    {
        [JsonProperty("response")]
        public string? Response { get; set; }
        
        [JsonProperty("done")]
        public bool Done { get; set; }
    }

    public class ModelsResponse
    {
        [JsonProperty("models")]
        public List<ModelInfo>? Models { get; set; }
    }

    public class ModelInfo
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
        
        [JsonProperty("size")]
        public long Size { get; set; }
        
        [JsonProperty("modified_at")]
        public string? ModifiedAt { get; set; }
    }
}