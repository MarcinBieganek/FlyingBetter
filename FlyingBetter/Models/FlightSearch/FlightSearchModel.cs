using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FlyingBetter.Models.FlightSearch
{
    public class FlightSearchModel
    {
        [Required(ErrorMessage = "Location From is required.")]
        [MinLength(2, ErrorMessage = "Too short.")]
        [MaxLength(50, ErrorMessage = "Too long.")]
        [RegularExpression(@"^[a-zA-Z''-'\s]+$", ErrorMessage = "Only letters")]
        public string From { get; set; }

        [Required(ErrorMessage = "Location To is required.")]
        [MinLength(2, ErrorMessage = "Too short.")]
        [MaxLength(50, ErrorMessage = "Too long.")]
        [RegularExpression(@"^[a-zA-Z''-'\s]+$", ErrorMessage = "Only letters")]
        public string To { get; set; }

        [Required(ErrorMessage = "Flight date is required")]
        [DataType(DataType.Date, ErrorMessage = "Format is in incorrect format.")]
        public DateTime Date { get; set; }
    }
}