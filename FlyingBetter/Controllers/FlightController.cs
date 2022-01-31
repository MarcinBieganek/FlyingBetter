using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.IO;
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
            FlightsResultOrder flightOrder = new FlightsResultOrder();

            await flightApi.GetCitiesInfo(resultModel);
            airports.AddNearestAirportsCodes(resultModel);
            await flightApi.GetNeededFlightsNearest(resultModel, 1);

            if (model.To == null || 
                    (model.FlightType != FlightTypes.OneWay.ToString() && 
                    model.FlightBackFrom == "NA"))
            {
                resultModel.SortFlightResults();
            }

            flightOrder.orderFlightResults(resultModel);

            return View(resultModel);
        }

        [HttpGet]
        public async Task<ActionResult> Ideas()
        {
            var model = new FlightIdeasModel();

            SearchHistory searchHistory = new SearchHistory();
            FlightApi flightApi = new FlightApi();
            FlightsResultOrder flightOrder = new FlightsResultOrder();
            model.PopularDest = searchHistory.getPopularDest();

            await flightApi.GetFlightsToPopularDest(model);

            flightOrder.orderFlightResults(model);
            flightOrder.limitFlightResults(model, 50);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Ideas(FlightIdeasModel model)
        {
            if (this.ModelState.IsValid)
            {
                SearchHistory searchHistory = new SearchHistory();
                FlightApi flightApi = new FlightApi();
                FlightsResultOrder flightOrder = new FlightsResultOrder();
                model.PopularDest = searchHistory.getPopularDest();

                await flightApi.GetCitiesInfo(model);
                await flightApi.GetFlightsToPopularDest(model);

                flightOrder.orderFlightResults(model);
                flightOrder.limitFlightResults(model, 50);

                return View(model);
            }

            return View(model);
        }
    }
}