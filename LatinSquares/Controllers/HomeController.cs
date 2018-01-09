using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static LatinSquares.Models.DbModels;

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

        public ActionResult DB()
        {
            ViewBag.Title = "Our Database";
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                ViewBag.empty = db.Rectangles.Count(x => x.Type == DbRectangle.TYPE_EMPTY);
                ViewBag.full = db.Rectangles.Count(x => x.Type == DbRectangle.TYPE_FULL);
                ViewBag.non_trivial = db.Rectangles.Count(x => x.Type == DbRectangle.TYPE_NON_TRIVIAL);
            }
            return View();
        }

        public ActionResult VisualSimulation()
        {
            ViewBag.Title = "Visual Simulation";
            return View();
        }

    }
}
