using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyingBetter.Models.Flight
{
    public class FlightSearchModel
    {
        [Required(ErrorMessage = "Location From is required.")]
        [MinLength(2, ErrorMessage = "Too short.")]
        [MaxLength(50, ErrorMessage = "Too long.")]
        [RegularExpression(@"^[a-zA-Z''-'\s]+$", ErrorMessage = "Only letters")]
        public string From { get; set; }

        [MaxLength(50, ErrorMessage = "Too long.")]
        [RegularExpression(@"^[a-zA-Z''-'\s]+$", ErrorMessage = "Only letters")]
        public string To { get; set; }

        [Required(ErrorMessage = "Flight date is required")]
        [DataType(DataType.Date, ErrorMessage = "Format is in incorrect format.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Skip Date indication is required")]
        public bool SkipDate { get; set; }

        [Required(ErrorMessage = "Adults number is required")]
        [Range(1, 20, ErrorMessage = "Adults number must be between 1 and 20")]
        public int Adults { get; set; }

        [Required(ErrorMessage = "Children number is required")]
        [Range(0, 20, ErrorMessage = "Children number must be between 0 and 20")]
        public int Children { get; set; }

        [Required(ErrorMessage = "Direct or indirect indication is required")]
        public bool Direct { get; set; }

        [Required(ErrorMessage = "Flight type is required")]
        public string FlightType { get; set; }

        [MaxLength(50, ErrorMessage = "Too long.")]
        [RegularExpression(@"^[a-zA-Z''-'\s]+$", ErrorMessage = "Only letters")]
        public string FlightBackFrom { get; set; }

        [Required(ErrorMessage = "Location To is required.")]
        [MinLength(2, ErrorMessage = "Too short.")]
        [MaxLength(50, ErrorMessage = "Too long.")]
        [RegularExpression(@"^[a-zA-Z''-'\s]+$", ErrorMessage = "Only letters")]
        public string FlightBackTo { get; set; }

        [Required(ErrorMessage = "Flight date is required")]
        [DataType(DataType.Date, ErrorMessage = "Format is in incorrect format.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FlightBackDate { get; set; }

        [Required(ErrorMessage = "Skip Date indication is required")]
        public bool SkipFlightBackDate { get; set; }

        [Required(ErrorMessage = "Adults number is required")]
        [Range(0, 20, ErrorMessage = "Adults number must be between 1 and 20")]
        public int FlightBackAdults { get; set; }

        [Required(ErrorMessage = "Children number is required")]
        [Range(0, 20, ErrorMessage = "Children number must be between 0 and 20")]
        public int FlightBackChildren { get; set; }

        [Required(ErrorMessage = "Direct or indirect indication is required")]
        public bool FlightBackDirect { get; set; }

        public FlightSearchModel()
        {
            this.Adults = 1;
            this.Date = DateTime.Now;
            this.FlightType = FlightTypes.OneWay.ToString();
            this.FlightBackDate = DateTime.Now.AddDays(1.0);
            this.FlightBackFrom = "NA";
            this.FlightBackTo = "NA";
        }
    }

    public enum FlightTypes
    {
        OneWay,
        RoundTripStandard,
        RoundTripNonStandard
    }
}