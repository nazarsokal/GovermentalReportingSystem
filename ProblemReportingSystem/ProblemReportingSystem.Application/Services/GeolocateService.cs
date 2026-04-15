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

            var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat}&lon={lon}&zoom=18&addressdetails=1";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "ProblemReportingSystem/1.0");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(jsonContent);

            var result = new ParsedAddressDto
            {
                FullAddress = data["display_name"]?.ToString() ?? "Unknown location"
            };

            var addressNode = data["address"];
            if (addressNode != null)
            {
                // Вулиця
                result.Street = addressNode["road"]?.ToString() 
                                ?? addressNode["street"]?.ToString() 
                                ?? addressNode["pedestrian"]?.ToString() 
                                ?? string.Empty;
    
                // Номер будинку
                result.BuildingNumber = addressNode["house_number"]?.ToString() ?? string.Empty;

                // Населений пункт (Nominatim по-різному тегає міста, містечка і села)
                result.City = addressNode["city"]?.ToString() 
                              ?? addressNode["town"]?.ToString() 
                              ?? addressNode["village"]?.ToString() 
                              ?? addressNode["hamlet"]?.ToString() 
                              ?? string.Empty;

                // Район (найчастіше в Україні це county)
                result.District = addressNode["county"]?.ToString() 
                                  ?? addressNode["district"]?.ToString() 
                                  ?? string.Empty;

                // Область (найчастіше це state)
                result.Oblast = addressNode["state"]?.ToString() 
                                ?? addressNode["province"]?.ToString() 
                                ?? string.Empty;

                // Країна
                result.Country = addressNode["country"]?.ToString() ?? string.Empty;

                // Поштовий індекс
                result.Postcode = addressNode["postcode"]?.ToString() ?? string.Empty;
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected error: " + ex.Message);
            throw;
        }
    }
}