using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

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
}