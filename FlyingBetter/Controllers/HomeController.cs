using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlyingBetter.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Help()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
    }
}