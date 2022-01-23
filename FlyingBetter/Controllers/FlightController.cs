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

            await flightApi.GetCitiesCodes(resultModel);
            await flightApi.GetNeededFlights(resultModel);

            // if did not found any flight at given dates try nearest dates
            if (resultModel.flightsResults.data.Count() == 0)
            {
                await flightApi.GetNeededFlightsAtNearestDates(resultModel);
            }
            if (!(resultModel.flightsBackResults is null) && resultModel.flightsBackResults.data.Count() == 0)
            {
                await flightApi.GetNeededFlightsBackAtNearestDates(resultModel);
            }

            return View(resultModel);
        }
    }
}