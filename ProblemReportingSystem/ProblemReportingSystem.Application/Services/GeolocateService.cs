using System.Globalization;
using Newtonsoft.Json.Linq;
using ProblemReportingSystem.Application.DTOs;
using ProblemReportingSystem.Application.ServiceAbstractions;

namespace ProblemReportingSystem.Application.Services;

public class GeolocateService : IGeolocateService
{
    private readonly HttpClient _httpClient;

    public GeolocateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ParsedAddressDto> GetAddressFromCoordinatesAsync(decimal latitude, decimal longitude)
    {
        try
        {
            string lat = latitude.ToString(CultureInfo.InvariantCulture);
            string lon = longitude.ToString(CultureInfo.InvariantCulture);

            // Основний запит: максимальна деталізація вулиць та будинків (zoom = 18)
            JObject data = await FetchNominatimDataAsync(lat, lon, 18);

            string displayName = data["display_name"]?.ToString() ?? "Unknown location";

            var result = new ParsedAddressDto
            {
                FullAddress = displayName
            };

            var addressNode = data["address"];
            if (addressNode != null)
            {
                result.Street = addressNode["road"]?.ToString() 
                                ?? addressNode["street"]?.ToString() 
                                ?? addressNode["pedestrian"]?.ToString() 
                                ?? string.Empty;
    
                result.BuildingNumber = addressNode["house_number"]?.ToString() ?? string.Empty;

                result.City = addressNode["city"]?.ToString() 
                              ?? addressNode["town"]?.ToString() 
                              ?? addressNode["village"]?.ToString() 
                              ?? addressNode["hamlet"]?.ToString() 
                              ?? string.Empty;

                result.District = addressNode["county"]?.ToString() 
                                  ?? addressNode["district"]?.ToString() 
                                  ?? string.Empty;

                result.Oblast = ExtractOblast(addressNode);

                result.Country = addressNode["country"]?.ToString() ?? string.Empty;
                result.Postcode = addressNode["postcode"]?.ToString() ?? string.Empty;
            }

            // ФОЛБЕК 1: Шукаємо область у загальному рядку адреси
            if (string.IsNullOrWhiteSpace(result.Oblast) && !string.IsNullOrWhiteSpace(displayName))
            {
                var parts = displayName.Split(',');
                var oblastPart = parts.FirstOrDefault(p => 
                    p.ToLower().Contains("область") || 
                    p.ToLower().Contains("oblast"));
                    
                if (oblastPart != null)
                {
                    result.Oblast = oblastPart.Trim();
                }
            }

            // ФОЛБЕК 2: Якщо область все ще порожня (як у випадку з Золочевом), 
            // робимо ширший запит виключно для адміністративних меж (zoom = 8)
            if (string.IsNullOrWhiteSpace(result.Oblast))
            {
                JObject broaderData = await FetchNominatimDataAsync(lat, lon, 8);
                var broaderAddressNode = broaderData["address"];
                if (broaderAddressNode != null)
                {
                    result.Oblast = ExtractOblast(broaderAddressNode);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected error in GeolocateService: " + ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Допоміжний метод для виконання HTTP запитів до Nominatim
    /// </summary>
    private async Task<JObject> FetchNominatimDataAsync(string lat, string lon, int zoom)
    {
        var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat}&lon={lon}&zoom={zoom}&addressdetails=1&accept-language=uk";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        // Nominatim вимагає User-Agent, інакше блокує запити
        request.Headers.Add("User-Agent", "ProblemReportingSystem/1.0");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        return JObject.Parse(jsonContent);
    }

    /// <summary>
    /// Витягує назву області з вузла address, враховуючи різні теги OSM
    /// </summary>
    private string ExtractOblast(JToken addressNode)
    {
        return addressNode["state"]?.ToString() 
            ?? addressNode["region"]?.ToString()
            ?? addressNode["state_district"]?.ToString()
            ?? addressNode["province"]?.ToString()
            ?? string.Empty;
    }
}