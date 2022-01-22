using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FlyingBetter.Models.Flight
{
    public class FlightSearchResultModel
    {
        public FlightSearchModel searchDetails { get; set; }
        public string fromCode { get; set; }

        public FlightSearchResultModel()
        {
            this.searchDetails = new FlightSearchModel();
        }

        public FlightSearchResultModel(FlightSearchModel flightSearchModel)
        {
            this.searchDetails = flightSearchModel;
        }
    }

    public class FlightApi
    {
        private HttpClient client;
        private string autocompleteBaseUri;
        
        public FlightApi()
        {
            this.client = new HttpClient();
            this.autocompleteBaseUri = "http://autocomplete.travelpayouts.com/places2";
        }

        public async Task<string> getCityCode(string cityName)
        {
            string queryParams = $"?term={cityName}&locale=en&types[]=city";
            var apiRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(this.autocompleteBaseUri + queryParams)
            };

            using (var apiResponse = await this.client.SendAsync(apiRequest))
            {
                if (apiResponse.IsSuccessStatusCode)
                {
                    List<AutocompleteResult> autocompleteResult = await apiResponse.Content.ReadFromJsonAsync<List<AutocompleteResult>>();

                    if (autocompleteResult.Count > 0)
                    {
                        return autocompleteResult[0].code;
                    }
                }
            }

            return null;
        }
    }

    public class AutocompleteResult
    {
        public string state_code { get; set; }
        public int weight { get; set; }
        public string name { get; set; }
        public string country_code { get; set; }
        public Dictionary<string, double> coordinates { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public string country_name { get; set; }
        public string country_cases { get; set; }
        public List<string> indexx_strings { get; set; }
        public string main_airport_name { get; set; }
        public Dictionary<string, string> cases { get; set; }
    }
}