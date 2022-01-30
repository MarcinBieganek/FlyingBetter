using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlyingBetter.Models.Flight;
using System.Threading.Tasks;

namespace FlyingBetter.Controllers
{
    public class FlightController : Controller
    {
        [HttpGet]
        public ActionResult Search()
        {
            var model = new FlightSearchModel();

            return View(model);
        }

        [HttpPost]
        public ActionResult Search(FlightSearchModel model)
        {
            if (this.ModelState.IsValid)
            {
                return RedirectToAction("Result", model);
            }
 
            return View(model);
        }

        public async Task<ActionResult> Result(FlightSearchModel model)
        {
            FlightSearchResultModel resultModel = new FlightSearchResultModel(model);
            FlightApi flightApi = new FlightApi();
            Airports airports = new Airports();

            await flightApi.GetCitiesInfo(resultModel);
            airports.AddNearestAirportsCodes(resultModel);
            await flightApi.GetNeededFlightsNearest(resultModel, 1);

            if (model.To == null || 
                    (model.FlightType != FlightTypes.OneWay.ToString() && 
                    model.FlightBackFrom == "NA"))
            {
                resultModel.SortFlightResults();
            }

            return View(resultModel);
        }
    }
}