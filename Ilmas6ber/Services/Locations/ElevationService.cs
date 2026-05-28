using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Ilmas6ber.Services.Locations
{
    public class ElevationService
    {
        private readonly HttpClient _http = new();

        public async Task<double?> GetElevation(double latitude, double longitude)
        {
            try
            {
                var url = $"https://api.open-meteo.com/v1/elevation?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}";
                var response = await _http.GetStringAsync(url);
                var doc = JsonDocument.Parse(response);
                return doc.RootElement.GetProperty("elevation")[0].GetDouble();
            }
            catch
            {
                return null;
            }
        }
    }
}
