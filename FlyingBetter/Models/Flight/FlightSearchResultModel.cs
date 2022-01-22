using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyingBetter.Models.Flight
{
    public class FlightSearchResultModel
    {
        public FlightSearchModel searchDetails { get; set; }

        public FlightSearchResultModel()
        {
            this.searchDetails = new FlightSearchModel();
        }

        public FlightSearchResultModel(FlightSearchModel flightSearchModel)
        {
            this.searchDetails = flightSearchModel;
        }
    }
}