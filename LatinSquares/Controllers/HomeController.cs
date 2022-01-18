using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
            ViewBag.Title = "Home";
            return View();
        }

        public ActionResult Full()
        {
            ViewBag.Title = "Full Rectangles";
            return View();
        }

        public ActionResult Empty()
        {
            ViewBag.Title = "Empty Rectangles";
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
                Dictionary<string, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, bool>>>>> dict =
                    new Dictionary<string, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, bool>>>>>();
                var list = db.Rectangles.Take(10000).ToList();
                foreach (var r in list)
                {
                    if (!dict.ContainsKey(r.Type))
                        dict.Add(r.Type, new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, bool>>>>());

                    if (!dict[r.Type].ContainsKey(r.Rows))
                        dict[r.Type].Add(r.Rows, new Dictionary<int, Dictionary<int, Dictionary<int, bool>>>());

                    if (!dict[r.Type][r.Rows].ContainsKey(r.Cols))
                        dict[r.Type][r.Rows].Add(r.Cols, new Dictionary<int, Dictionary<int, bool>>());

                    if (!dict[r.Type][r.Rows][r.Cols].ContainsKey(r.Symbols))
                        dict[r.Type][r.Rows][r.Cols].Add(r.Symbols, new Dictionary<int, bool>());

                    if (!dict[r.Type][r.Rows][r.Cols][r.Symbols].ContainsKey(r.Count))
                        dict[r.Type][r.Rows][r.Cols][r.Symbols].Add(r.Count, true);
                }
                ViewBag.info = JsonConvert.SerializeObject(dict);
            }
            return View();
        }

        public ActionResult VisualSimulation()
        {
            ViewBag.Title = "Visual Simulation";
            return View();
        }

        public ActionResult Documentation()
        {
            ViewBag.Title = "Documentation";
            return View();
        }

    }
}
