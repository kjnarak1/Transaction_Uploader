using Newtonsoft.Json;
using Transaction_Uploader.Models;

namespace Transaction_Uploader.Services
{
    public static class GetCurrencyCode
    {
        public static async Task<List<Currency>> LoadCurrenciesAsync(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<List<Currency>>(json);
            }
        }
    }
}
