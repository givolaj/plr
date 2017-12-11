using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LatinSquares.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            return View();
        }

        public ActionResult Full()
        {
            ViewBag.Title = "Full Rectangles";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title = "About";
            return View();
        }

        public ActionResult Investigation()
        {
            ViewBag.Title = "Rectangles Investigation";
            return View();
        }

        public ActionResult NonTrivial()
        {
            ViewBag.Title = "Non Trivial Rectangles";
            return View();
        }
    }
}
