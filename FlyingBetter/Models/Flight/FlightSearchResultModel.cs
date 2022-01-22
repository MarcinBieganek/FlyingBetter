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
        public string toCode { get; set; }
        public string FlightBackFromCode { get; set; }
        public string FlightBackToCode { get; set; }
        public bool success { get; set; }
        public string errorDescription { get; set; }

        public FlightSearchResultModel()
        {
            this.searchDetails = new FlightSearchModel();
            this.success = true;
        }

        public FlightSearchResultModel(FlightSearchModel flightSearchModel)
        {
            this.searchDetails = flightSearchModel;
            this.success = true;
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
                    throw new NoAirportFoundException(cityName);
                }
            }        
            throw new GetCityCodeException();
        }

        public async Task GetCitiesCodes(FlightSearchResultModel model)
        {
            try
            {
                model.fromCode = await getCityCode(model.searchDetails.From);
                model.toCode = await getCityCode(model.searchDetails.To);
                model.FlightBackFromCode = model.toCode;
                model.FlightBackToCode = model.fromCode;
                if (model.searchDetails.FlightType == FlightTypes.RoundTripNonStandard.ToString())
                {
                    if (model.searchDetails.FlightBackFrom == model.searchDetails.To)
                    {
                        model.FlightBackFromCode = model.toCode;
                    } else
                    {
                        model.FlightBackFromCode = await getCityCode(model.searchDetails.FlightBackFrom);
                    }
                    if (model.searchDetails.FlightBackTo == model.searchDetails.From)
                    {
                        model.FlightBackToCode = model.fromCode;
                    }
                    else
                    {
                        model.FlightBackToCode = await getCityCode(model.searchDetails.FlightBackTo);
                    }
                }
            } catch (NoAirportFoundException e)
            {
                model.success = false;
                model.errorDescription = $"We did not found airport for {e.Message}.";
            }
            catch (GetCityCodeException e)
            {
                model.success = false;
                model.errorDescription = "Error occurred, try again in a moment.";
            }
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

    public class NoAirportFoundException : Exception
    {
        public NoAirportFoundException(string m) : base(m)
        {
        }
    }
    public class GetCityCodeException : Exception
    {
    }
}