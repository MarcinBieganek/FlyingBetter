using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlyingBetter.Models.FlightSearch;

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

        public ActionResult Result(FlightSearchModel model)
        {

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}