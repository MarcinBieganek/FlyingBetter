using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace FlyingBetter.Models.Flight
{
    public class FlightSearchResultModel
    {
        public FlightSearchModel searchDetails { get; set; }
        public AutocompleteResult fromInfo { get; set; }
        public AutocompleteResult toInfo { get; set; }
        public AutocompleteResult flightBackFromInfo { get; set; }
        public AutocompleteResult flightBackToInfo { get; set; }
        public List<string> fromCodes { get; set; }
        public List<string> toCodes { get; set; }
        public List<string> flightBackFromCodes { get; set; }
        public List<string> flightBackToCodes { get; set; }
        public List<FlightsResult> flightsResults { get; set; }
        public List<FlightsResult> flightsBackResults { get; set; }
        public bool flightsNearestDayChecked { get; set; }
        public bool flightsBackNearestDayChecked { get; set; }
        public bool success { get; set; }
        public string errorDescription { get; set; }

        public FlightSearchResultModel()
        {
            this.searchDetails = new FlightSearchModel();
            this.success = true;
            this.flightsNearestDayChecked = false;
            this.fromCodes = new List<string>();
            this.toCodes = new List<string>();
            this.flightBackFromCodes = new List<string>();
            this.flightBackToCodes = new List<string>();
            this.flightsResults = new List<FlightsResult>();
            this.flightsBackResults = new List<FlightsResult>();
        }

        public FlightSearchResultModel(FlightSearchModel flightSearchModel)
        {
            this.searchDetails = flightSearchModel;
            this.success = true;
            this.flightsNearestDayChecked = false;
            this.fromCodes = new List<string>();
            this.toCodes = new List<string>();
            this.flightBackFromCodes = new List<string>();
            this.flightBackToCodes = new List<string>();
            this.flightsResults = new List<FlightsResult>();
            this.flightsBackResults = new List<FlightsResult>();
        }

        public void SortFlightResults()
        {
            this.flightsResults = 
                this.flightsResults
                    .OrderBy(fr => fr.destination_airport)
                    .ToList();

            this.flightsBackResults = 
                this.flightsBackResults
                    .OrderBy(fr => fr.origin_airport)
                    .ToList();
        }
    }

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
                    // wait for the async results
                    if (model.searchDetails.SkipFlightBackDate)
                    {
                        for (int i = 0; i < model.fromCodes.Count() * model.toCodes.Count(); i++)
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
                        for (int i = 0; i < model.flightBackFromCodes.Count() * model.flightBackToCodes.Count() * daysDiffs.Count(); i++)
                        {
                            model.flightsBackResults.AddRange((await getFlightsBackTasks[i]).data);
                        }
                    }
                }
                // wait for the async results
                if (model.searchDetails.SkipDate)
                {
                    for (int i = 0; i < model.fromCodes.Count() * model.toCodes.Count(); i++)
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
                    for (int i = 0; i < model.fromCodes.Count() * model.toCodes.Count() * daysDiffs.Count(); i++)
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

        public async Task GetGroupedFlightsNearest(FlightSearchResultModel model)
        {
            try
            {
                // get first flights
                List<Task<FlightsGroupedResults>> getGroupedFlightsTasks = new List<Task<FlightsGroupedResults>>();
                foreach (var fromCode in model.fromCodes)
                {
                    foreach (var toCode in model.toCodes)
                    {
                        getGroupedFlightsTasks.Add(GetGroupedFlights(fromCode, toCode, model.searchDetails.Direct));
                    }
                }
                
                // wait for the async results
                for (int i = 0; i < model.fromCodes.Count() * model.toCodes.Count(); i++)
                {
                    FlightsGroupedResults fgr = await getGroupedFlightsTasks[i];
                    model.flightsResults.AddRange(fgr.ToFlightsResults().data);
                }
            }
            catch (ApiCallException e)
            {
                model.success = false;
                model.errorDescription = "Error occurred, try again in a moment.";
            }
        }
    }

    public class Airports
    {
        public static List<Airport> airports;

        static Airports()
        {
            using (var airportsReader = new StreamReader(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "App_Data/airports.csv")))
            using (var airportsCsvReader = new CsvReader(airportsReader, CultureInfo.InvariantCulture))
            {
                airports = airportsCsvReader.GetRecords<Airport>().ToList();
            }
        }

        private double DegreesToRadians(double degrees)
        {
            return (degrees * Math.PI) / 180.0;
        }

        private double GeoDistance(double latX, double lonX, double latY, double lonY)
        {
            double latXRad = DegreesToRadians(latX);
            double lonXRad = DegreesToRadians(lonX);
            double latYRad = DegreesToRadians(latY);
            double lonYRad = DegreesToRadians(lonY);

            // the haversine formula
            return 2.0 * 6371.0 *
                    Math.Asin(
                        Math.Sqrt(
                            Math.Pow(
                                Math.Sin((latYRad - latXRad) / 2),
                                2
                            ) +
                            (Math.Cos(latXRad) *
                             Math.Cos(latYRad) *
                             Math.Pow(
                                Math.Sin((lonYRad - lonXRad) / 2),
                                2
                             ))
                        )
                    );
        }

        public List<string> GetNearestAirportsCodes(double lat, double lon, double dist)
        {
            return (from airport in airports
                    where (airport.type == "large_airport" || airport.type == "medium_airport") && (airport.iata_code != "") && (airport.scheduled_service == "yes")
                    where GeoDistance(airport.latitude_deg, airport.longitude_deg, lat, lon) < dist
                    orderby GeoDistance(airport.latitude_deg, airport.longitude_deg, lat, lon)
                    select airport.iata_code).Take<string>(5).ToList();
        }

        public void AddNearestAirportsCodes(FlightSearchResultModel model)
        {
            model.fromCodes.AddRange(GetNearestAirportsCodes(model.fromInfo.coordinates["lat"], model.fromInfo.coordinates["lon"], 200.0));
            model.fromCodes = model.fromCodes.Distinct().Take<string>(5).ToList();
            // To city only if was specified
            if (model.searchDetails.To != null)
            {
                model.toCodes.AddRange(GetNearestAirportsCodes(model.toInfo.coordinates["lat"], model.toInfo.coordinates["lon"], 200.0));
                model.toCodes = model.toCodes.Distinct().Take<string>(5).ToList();
            }

            if (model.searchDetails.FlightType != FlightTypes.OneWay.ToString())
            {
                if (model.searchDetails.FlightBackFrom != "NA")
                {
                    model.flightBackFromCodes.AddRange(GetNearestAirportsCodes(model.flightBackFromInfo.coordinates["lat"], model.flightBackFromInfo.coordinates["lon"], 200.0));
                    model.flightBackFromCodes = model.flightBackFromCodes.Distinct().ToList();
                }
                model.flightBackToCodes.AddRange(GetNearestAirportsCodes(model.flightBackToInfo.coordinates["lat"], model.flightBackToInfo.coordinates["lon"], 200.0));
                model.flightBackToCodes = model.flightBackToCodes.Distinct().ToList();
            }
        }
    }

    public class Airport
    {
        public int id { get; set; }
        public string ident { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public double latitude_deg { get; set; }
        public double longitude_deg { get; set; }
        public string elevation_ft { get; set; }
        public string continent { get; set; }
        public string iso_country { get; set; }
        public string iso_region { get; set; }
        public string municipality { get; set; }
        public string scheduled_service { get; set; }
        public string gps_code { get; set; }
        public string iata_code { get; set; }
        public string local_code { get; set; }
        public string home_link { get; set; }
        public string wikipedia_link { get; set; }
        public string keywords { get; set; }
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

    public class FlightsResultOrder
    { 
        public int FlightValue(FlightsResult flight)
        {
            int value = -flight.price;
            // flights without layovers are better
            if (flight.transfers == 0)
            {
                value += 100;
            } else
            {
                value -= (flight.transfers * 50);
            }
            // flights during a day are better
            TimeSpan dayStart = new TimeSpan(8, 0, 0);
            TimeSpan dayEnd = new TimeSpan(20, 0, 0);
            if (dayStart <= flight.departure_at.TimeOfDay && flight.departure_at.TimeOfDay <= dayEnd)
            {
                value += 100;
            }
            // last minute flights are worst
            TimeSpan oneMonth = new TimeSpan(30, 0, 0, 0);
            TimeSpan fourMonths = new TimeSpan(120, 0, 0, 0);
            TimeSpan nowToFlightDiff = flight.departure_at - DateTime.Now;
            if (nowToFlightDiff < oneMonth)
            {
                value += 30;
            }
            else if (oneMonth <= nowToFlightDiff && nowToFlightDiff <= fourMonths)
            {
                value += 100;
            } 
            else
            {
                value += 50;
            }

            return value;
        }

        public List<FlightsResult> orderFlights(List<FlightsResult> flights)
        {
            return flights
                    .OrderByDescending(f => FlightValue(f))
                    .ToList();
        }

        public void orderFlightResults(FlightSearchResultModel model)
        {
            model.flightsResults = orderFlights(model.flightsResults);
            model.flightsBackResults = orderFlights(model.flightsBackResults);
        }
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