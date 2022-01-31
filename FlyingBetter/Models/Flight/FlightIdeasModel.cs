using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyingBetter.Models.Flight
{
    public class FlightIdeasModel
    {
        [MaxLength(50, ErrorMessage = "Too long.")]
        [RegularExpression(@"^[a-zA-Z''-'\s]+$", ErrorMessage = "Only letters")]
        public string From { get; set; }
        [Required(ErrorMessage = "Direct or indirect indication is required")]
        public bool Direct { get; set; }

        public string fromCode { get; set; }
        public List<FlightsResult> FlightsResults { get; set; }

        public List<string> PopularDest { get; set; }

        public bool success { get; set; }
        public string errorDescription { get; set; }

        public FlightIdeasModel()
        {
            this.FlightsResults = new List<FlightsResult>();
            this.success = true;
        }
    }
}