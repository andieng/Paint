using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;

namespace Paint
{
    public class ResultsObject
    {
        [JsonProperty("images")]
        public string[] images { get; set; }
    }
    public class ApiResponse
    {
        [JsonProperty("results")]
        public ResultsObject results { get; set; }
    }
    public class TextToImage
    {
        private static Lazy<TextToImage> instance = new Lazy<TextToImage>(() => new TextToImage());
        private string _apiUrl = "";
        private string _apiKey = "";
        private string _apiHost = "";

        private readonly HttpClient httpClient;
        public TextToImage()
        {
            _apiUrl = ConfigurationManager.AppSettings["TextToImageUrl"];
            _apiKey = ConfigurationManager.AppSettings["XAPIKey"];
            _apiHost = ConfigurationManager.AppSettings["XAPIHost"];

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", _apiKey);
            httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", _apiHost);
        }
        public static TextToImage Instance => instance.Value;

        public async Task<string> MakeApiCallAsync(string prompt,int page)
        {
            var parameters = new
            {
                prompt,
                page
            };

            try
            {
                string jsonData = JsonConvert.SerializeObject(parameters);
                var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

                // Make the API call
                HttpResponseMessage response = await httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<ApiResponse>(data);

                    string[] dataArray = responseObject?.results.images;
                    return dataArray[0];
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }
    }
}
