using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FlyingBetter.Models.Flight
{
    public class FlightSearchResultModel
    {
        public FlightSearchModel searchDetails { get; set; }
        public string fromCode { get; set; }
        public string toCode { get; set; }
        public string flightBackFromCode { get; set; }
        public string flightBackToCode { get; set; }
        public FlightsResults flightsResults { get; set; }
        public FlightsResults flightsBackResults { get; set; }
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
        private string flightsBaseUri;
        
        public FlightApi()
        {
            this.client = new HttpClient();
            this.autocompleteBaseUri = "http://autocomplete.travelpayouts.com/places2";
            this.flightsBaseUri = "https://api.travelpayouts.com/aviasales/v3/prices_for_dates";
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
                    List<AutocompleteResult> autocompleteResults = await apiResponse.Content.ReadFromJsonAsync<List<AutocompleteResult>>();

                    if (autocompleteResults.Count > 0)
                    {
                        return autocompleteResults[0].code;
                    }
                    throw new NoAirportFoundException(cityName);
                }
            }        
            throw new ApiCallException();
        }

        public async Task GetCitiesCodes(FlightSearchResultModel model)
        {
            try
            {
                // get first flight cities codes
                var getCityCodeFrom = getCityCode(model.searchDetails.From);
                var getCityCodeTo = getCityCode(model.searchDetails.To);
                model.fromCode = await getCityCodeFrom;
                model.toCode = await getCityCodeTo;
                // get flight back cities codes if needed
                model.flightBackFromCode = model.toCode;
                model.flightBackToCode = model.fromCode;
                if (model.searchDetails.FlightType == FlightTypes.RoundTripNonStandard.ToString())
                {
                    if (model.searchDetails.FlightBackFrom == model.searchDetails.To)
                    {
                        model.flightBackFromCode = model.toCode;
                    } else
                    {
                        model.flightBackFromCode = await getCityCode(model.searchDetails.FlightBackFrom);
                    }
                    if (model.searchDetails.FlightBackTo == model.searchDetails.From)
                    {
                        model.flightBackToCode = model.fromCode;
                    }
                    else
                    {
                        model.flightBackToCode = await getCityCode(model.searchDetails.FlightBackTo);
                    }
                }
            } catch (NoAirportFoundException e)
            {
                model.success = false;
                model.errorDescription = $"We did not found airport for {e.Message}.";
            } catch (ApiCallException e)
            {
                model.success = false;
                model.errorDescription = "Error occurred, try again in a moment.";
            }
        }

        public async Task<FlightsResults> getFlights(string fromCode, string toCode, DateTime date)
        {
            string queryParams = $"?origin={fromCode}&destination={toCode}&departure_at={date.ToString("yyyy-MM-dd")}&sorting=price&direct=true&currency=pln&limit=20&page=1&one_way=true&token=d7c205222fc0dfdc0a9054f1f5f8a7ea";
            var apiRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(this.flightsBaseUri + queryParams)
            };

            using (var apiResponse = await this.client.SendAsync(apiRequest))
            {
                if (apiResponse.IsSuccessStatusCode)
                {
                    return await apiResponse.Content.ReadFromJsonAsync<FlightsResults>();
                }
            }
            throw new ApiCallException();
        }

        public async Task getNeededFlights(FlightSearchResultModel model)
        {
            try
            {
                // get first flights
                Task<FlightsResults> getFlightsTask = getFlights(model.fromCode, model.toCode, model.searchDetails.Date);
                // get flights back if needed
                if (model.searchDetails.FlightType != FlightTypes.OneWay.ToString())
                {
                    model.flightsBackResults = await getFlights(model.flightBackFromCode, model.flightBackToCode, model.searchDetails.FlightBackDate);
                }
                model.flightsResults = await getFlightsTask;
            } catch(ApiCallException e)
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

    public class FlightsResults
    {
        public bool success { get; set; }
        public List<FlightsResult> data { get; set; }
        public string currency { get; set; }
    }

    public class FlightsResult
    {
        public string origin { get; set; }
        public string destination { get; set; }
        public string origin_airport { get; set; }
        public string destination_airport { get; set; }
        public int price { get; set; }
        public string airline { get; set; }
        public string flight_number { get; set; }
        public string departure_at { get; set; }
        public int transfers { get; set; }
        public int return_transfers { get; set; }
        public int duration { get; set; }
        public string link { get; set; }
    }

    public class NoAirportFoundException : Exception
    {
        public NoAirportFoundException(string m) : base(m)
        {
        }
    }
    public class ApiCallException : Exception
    {
    }
}