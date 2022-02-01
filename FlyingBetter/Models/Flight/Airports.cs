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
}