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

            await flightApi.getNeededFlights(resultModel);

            //resultModel.flightsResults = await flightApi.getFlights(resultModel.fromCode, resultModel.toCode, resultModel.searchDetails.Date);

            return View(resultModel);
        }
    }
}