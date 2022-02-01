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
    public class FlightApi
    {
        private HttpClient client;
        private string autocompleteBaseUri;
        private string flightsBaseUri;
        private string flightsGroupedBaseUri;

        public FlightApi()
        {
            this.client = new HttpClient();
            this.autocompleteBaseUri = "http://autocomplete.travelpayouts.com/places2";
            this.flightsBaseUri = "https://api.travelpayouts.com/aviasales/v3/prices_for_dates";
            this.flightsGroupedBaseUri = "https://api.travelpayouts.com/aviasales/v3/grouped_prices";
        }

        public async Task<AutocompleteResult> GetCityInfo(string cityName)
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
                        return autocompleteResults[0];
                    }
                    throw new NoAirportFoundException(cityName);
                }
            }
            throw new ApiCallException();
        }

        public async Task GetCitiesInfo(FlightSearchResultModel model)
        {
            try
            {
                // get first flight cities info and codes
                var getCityInfoFrom = GetCityInfo(model.searchDetails.From);
                // To city only if it was specified
                if (model.searchDetails.To != null)
                {
                    model.toInfo = await GetCityInfo(model.searchDetails.To);
                    model.toCodes.Add(model.toInfo.code);
                }
                else
                {
                    model.toCodes.Add("");
                }
                model.fromInfo = await getCityInfoFrom;
                model.fromCodes.Add(model.fromInfo.code);
                // get flight back cities info and codes if needed
                if (model.searchDetails.To != null)
                {
                    model.flightBackFromInfo = model.toInfo;
                    model.flightBackFromCodes.Add(model.flightBackFromInfo.code);
                }
                else
                {
                    model.flightBackFromCodes.Add("");
                }
                model.flightBackToInfo = model.fromInfo;
                model.flightBackToCodes.Add(model.flightBackToInfo.code);
                if (model.searchDetails.FlightType == FlightTypes.RoundTripNonStandard.ToString())
                {
                    if (model.searchDetails.FlightBackFrom != model.searchDetails.To)
                    {
                        if (model.searchDetails.FlightBackFrom != null)
                        {
                            model.flightBackFromInfo = await GetCityInfo(model.searchDetails.FlightBackFrom);
                            model.flightBackFromCodes[0] = model.flightBackFromInfo.code;
                        }
                        else
                        {
                            model.flightBackFromCodes[0] = "";
                        }
                    }
                    if (model.searchDetails.FlightBackTo != model.searchDetails.From)
                    {
                        model.flightBackToInfo = await GetCityInfo(model.searchDetails.FlightBackTo);
                        model.flightBackToCodes[0] = model.flightBackToInfo.code;
                    }
                }
            }
            catch (NoAirportFoundException e)
            {
                model.success = false;
                model.errorDescription = $"We did not found airport for {e.Message}.";
            }
            catch (ApiCallException e)
            {
                model.success = false;
                model.errorDescription = "Error occurred, try again in a moment.";
            }
        }

        public async Task GetCitiesInfo(FlightIdeasModel model)
        {
            try
            {
                if (model.From != null)
                {
                    model.fromCode = (await GetCityInfo(model.From)).code;
                }
                else
                {
                    model.fromCode = "";
                }
            }
            catch (NoAirportFoundException e)
            {
                model.success = false;
                model.errorDescription = $"We did not found airport for {e.Message}.";
            }
            catch (ApiCallException e)
            {
                model.success = false;
                model.errorDescription = "Error occurred, try again in a moment.";
            }
        }

        public async Task<FlightsResults> GetFlights(string fromCode, string toCode, DateTime date, bool direct)
        {
            string queryParams = $"?origin={fromCode}&destination={toCode}&departure_at={date.ToString("yyyy-MM-dd")}&sorting=price&direct={direct.ToString().ToLower()}&currency=pln&limit=20&page=1&one_way=true&token=d7c205222fc0dfdc0a9054f1f5f8a7ea";
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

        public async Task GetNeededFlightsNearest(FlightSearchResultModel model, int daysRangeRadius)
        {
            List<int> daysDiffs = Enumerable.Range(-daysRangeRadius, (daysRangeRadius * 2) + 1).ToList();
            try
            {
                // get first flights
                List<Task<FlightsResults>> getFlightsTasks = new List<Task<FlightsResults>>();
                List<Task<FlightsGroupedResults>> getFlightsGroupedTasks = new List<Task<FlightsGroupedResults>>();
                if (model.searchDetails.SkipDate)
                {
                    foreach (var fromCode in model.fromCodes)
                    {
                        foreach (var toCode in model.toCodes)
                        {
                            if (fromCode != toCode)
                                getFlightsGroupedTasks.Add(GetGroupedFlights(fromCode, toCode, model.searchDetails.Direct));
                        }
                    }
                }
                else
                {
                    foreach (var fromCode in model.fromCodes)
                    {
                        foreach (var toCode in model.toCodes)
                        {
                            foreach (var daysDiffOne in daysDiffs)
                            {
                                if (fromCode != toCode)
                                {
                                    // prepare new date
                                    DateTime newDate = model.searchDetails.Date.AddDays((double)daysDiffOne);
                                    // check if new date is okay
                                    if (DateTime.Today <= newDate &&
                                        (model.searchDetails.FlightType == FlightTypes.OneWay.ToString() ||
                                            model.searchDetails.SkipFlightBackDate ||
                                            newDate <= model.searchDetails.FlightBackDate))
                                    {
                                        getFlightsTasks.Add(GetFlights(fromCode, toCode, newDate, model.searchDetails.Direct));
                                    }
                                }
                            }
                        }
                    }
                }
                // get flights back if needed
                List<Task<FlightsResults>> getFlightsBackTasks = new List<Task<FlightsResults>>();
                List<Task<FlightsGroupedResults>> getFlightsBackGroupedTasks = new List<Task<FlightsGroupedResults>>();
                if (model.searchDetails.FlightType != FlightTypes.OneWay.ToString())
                {
                    // add destination airports from first flight results if there is
                    // no from location for flight back specified
                    if (model.searchDetails.FlightBackFrom == null)
                    {
                        foreach (var flightResult in model.flightsResults)
                        {
                            model.flightBackFromCodes.Add(flightResult.destination_airport);
                        }
                    }
                    if (model.searchDetails.SkipFlightBackDate)
                    {
                        foreach (var fromCode in model.flightBackFromCodes)
                        {
                            foreach (var toCode in model.flightBackToCodes)
                            {
                                if (fromCode != toCode)
                                    getFlightsBackGroupedTasks.Add(GetGroupedFlights(fromCode, toCode, model.searchDetails.FlightBackDirect));
                            }
                        }
                    }
                    else
                    {
                        foreach (var fromCode in model.flightBackFromCodes)
                        {
                            foreach (var toCode in model.flightBackToCodes)
                            {
                                foreach (var daysDiffOne in daysDiffs)
                                {
                                    if (fromCode != toCode)
                                    { 
                                        // prepare new date
                                        DateTime newDate = model.searchDetails.FlightBackDate.AddDays((double)daysDiffOne);
                                        // check if new date is okay
                                        if (model.searchDetails.SkipDate ||
                                            model.searchDetails.Date <= newDate)
                                        {
                                            getFlightsBackTasks.Add(GetFlights(fromCode, toCode, newDate, model.searchDetails.Direct));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // wait for the async results
                    if (model.searchDetails.SkipFlightBackDate)
                    {
                        for (int i = 0; i < getFlightsBackGroupedTasks.Count(); i++)
                        {
                            FlightsGroupedResults flightsBackGroupedResults = await getFlightsBackGroupedTasks[i];
                            if (model.searchDetails.FlightType != FlightTypes.OneWay.ToString() &&
                                (!model.searchDetails.SkipDate))
                            {
                                flightsBackGroupedResults.FilterResultsAfterDate(model.searchDetails.Date);
                            }
                            model.flightsBackResults.AddRange(flightsBackGroupedResults.ToFlightsResults().data);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < getFlightsBackTasks.Count(); i++)
                        {
                            model.flightsBackResults.AddRange((await getFlightsBackTasks[i]).data);
                        }
                    }
                }
                // wait for the async results
                if (model.searchDetails.SkipDate)
                {
                    for (int i = 0; i < getFlightsGroupedTasks.Count(); i++)
                    {
                        FlightsGroupedResults flightsGroupedResults = await getFlightsGroupedTasks[i];
                        if (model.searchDetails.FlightType != FlightTypes.OneWay.ToString() &&
                            (!model.searchDetails.SkipFlightBackDate))
                        {
                            flightsGroupedResults.FilterResultsBeforeDate(model.searchDetails.FlightBackDate);
                        }
                        model.flightsResults.AddRange(flightsGroupedResults.ToFlightsResults().data);
                    }
                }
                else
                {
                    for (int i = 0; i < getFlightsTasks.Count(); i++)
                    {
                        model.flightsResults.AddRange((await getFlightsTasks[i]).data);
                    }
                }
            }
            catch (ApiCallException e)
            {
                model.success = false;
                model.errorDescription = "Error occurred, try again in a moment.";
            }
        }

        public async Task<FlightsGroupedResults> GetGroupedFlights(string fromCode, string toCode, bool direct)
        {
            string queryParams = $"?origin={fromCode}&destination={toCode}&direct={direct.ToString().ToLower()}&currency=pln&token=d7c205222fc0dfdc0a9054f1f5f8a7ea";
            var apiRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(this.flightsGroupedBaseUri + queryParams)
            };

            using (var apiResponse = await this.client.SendAsync(apiRequest))
            {
                if (apiResponse.IsSuccessStatusCode)
                {
                    return await apiResponse.Content.ReadFromJsonAsync<FlightsGroupedResults>();
                }
            }
            throw new ApiCallException();
        }

        public async Task GetFlightsToPopularDest(FlightIdeasModel model)
        {
            try
            {
                // get first flights
                List<Task<FlightsGroupedResults>> getGroupedFlightsTasks = new List<Task<FlightsGroupedResults>>();
                foreach (var toCode in model.PopularDest)
                {
                    if (model.fromCode != toCode)
                        getGroupedFlightsTasks.Add(GetGroupedFlights(model.fromCode, toCode, model.Direct));
                }

                // wait for the async results
                for (int i = 0; i < getGroupedFlightsTasks.Count(); i++)
                {
                    FlightsGroupedResults fgr = await getGroupedFlightsTasks[i];
                    model.FlightsResults.AddRange(fgr.ToFlightsResults().data);
                }
            }
            catch (ApiCallException e)
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

    public class FlightsGroupedResults
    {
        public bool success { get; set; }
        public Dictionary<DateTime, FlightsResult> data { get; set; }
        public string currency { get; set; }

        public FlightsResults ToFlightsResults()
        {
            FlightsResults fr = new FlightsResults();
            fr.success = this.success;
            fr.data = this.data.Values.ToList();
            fr.currency = this.currency;

            return fr;
        }
        public void FilterResultsBeforeDate(DateTime date)
        {
            this.data = data
                        .Where(fr => fr.Key < date)
                        .ToDictionary(p => p.Key, p => p.Value);
        }
        public void FilterResultsAfterDate(DateTime date)
        {
            this.data = data
                        .Where(fr => fr.Key > date)
                        .ToDictionary(p => p.Key, p => p.Value);
        }
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
        public DateTime departure_at { get; set; }
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