using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using MvcQIS.Models;
using MvcJqGrid;

namespace MvcQIS.Controllers
{
    public class HomeController : Controller
    {
        TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();
        QISEntities dbQIS = new QISEntities();

        public ActionResult Index()
        {
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Message = "Main Menu";

            var listPlant = dbQIS.Master_Plant.Select(s => new
            {
                value = s.plant_id,
                text = s.plant_name
            }).ToList();

            ViewBag.SelectPlant = new SelectList(listPlant, "value", "text");

            return View();
        }

        public JsonResult GetTNCUser(GridSettings gridSettings)
        {
            var data = dbTNC.tnc_user.OrderBy(gridSettings.SortColumn + " " + gridSettings.SortOrder);
            // Count All Record
            double count = data.Count();

            // Prepare JSON Format
            object jsonData = new
            {
                page = gridSettings.PageIndex,
                total = Math.Ceiling(count / gridSettings.PageSize),
                records = count,
                rows = data.Select(d => new
                {
                    d.emp_code,
                    d.emp_fname,
                    d.emp_lname,
                    d.emp_sex,
                    d.email
                }).Skip((gridSettings.PageIndex - 1) * gridSettings.PageSize).Take(gridSettings.PageSize)
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCriticalReject(GridSettings gridSettings)
        {
            var data = dbQIS.QCReject.Where(w => w.defective_lv != null && w.defective_lv == "Critical").OrderBy(gridSettings.SortColumn + " " + gridSettings.SortOrder);

            // Count All Record
            double count = data.Count();

            // Prepare JSON Format
            object jsonData = new
            {
                page = gridSettings.PageIndex,
                total = Math.Ceiling(count / gridSettings.PageSize),
                records = count,
                rows = data.Select(d => new
                {
                    d.Car.responsible,
                    d.Car.report_no,
                    d.Master_Product.product_name,
                    d.item_code,
                    d.problem,
                    d.defective_lv,
                    d.picture,
                    d.Car.issued_date
                }).Skip((gridSettings.PageIndex - 1) * gridSettings.PageSize).Take(gridSettings.PageSize)
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult QCRejectHome()
        {
            var plantid = int.Parse(Session["PlantID"].ToString());
            var db = (from m in dbQIS.Main
                      where m.plant_id == plantid
                      select m);
            return View(db);
        }

        public ActionResult AddQCReject()
        {
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Message = "Main Menu";

            var plantid = int.Parse(Session["PlantID"].ToString());
            var db = (from m in dbQIS.Master_Product
                      where m.plant_id == plantid
                      select m);

            return View(db);
        }

        public ActionResult EditQCReject(string id, string key2)
        {
            
            //var db = (from m in dbQIS.QCReject
            //          where m.e
            return View();
        }

        public ActionResult Report()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact";

            return View();
        }
    }
}
