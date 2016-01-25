using MvcQIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using MvcJqGrid;
using System.Data.Entity;
using System.Data.Objects;

namespace MvcQIS.Controllers
{
    public class HomeController : Controller
    {
        TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();
        QISEntities dbQIS = new QISEntities();

        public ActionResult Index()
        {
            ViewBag.Title = "Home";
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Message = "Main Menu";
            
            var plant_mas = (from p in dbQIS.Master_Plant
                             select p);
            ViewBag.SelectPlant = plant_mas;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title = "Purpose";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Title = "Contact Us";
            return View();
        }

        public ActionResult GetTNCUser(GridSettings gridSettings)
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

        public ActionResult GridCriticalRejData(string sidx, string sord, int page, int rows, string search, int plantid)
        {
            var dbSel = from d in dbQIS.QCReject.ToList().Where(w => w.issue_car == 1 
                        && (w.defective_lv == "Critical" && w.status_id == 4)
                        && (w.Approved_Log.approved_dt >= DateTime.Now.AddDays(-9) || w.Car.reply_date == null))
                        join t in dbTNC.tnc_group_master.ToList() on d.Car.responsible equals t.id into j
                        from t in j.DefaultIfEmpty()
                        select new { groupname = t.group_name == null ? "N/A" : t.group_name, qc = d };

            if (plantid != 0)
            {
                dbSel = dbSel.Where(q => q.qc.plant_id == plantid);
            }
            dbSel = dbSel.OrderByDescending(q => q.qc.entry_date);

            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbSel.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            // This is possible because I'm using the LINQ Dynamic Query Library
            var qcrejects = dbSel.AsQueryable()
                    .OrderBy(sidx + " " + sord)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize).AsEnumerable();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = qcrejects.ToList().Select(q => new
                {
                    i = q.qc.qc_reject_id,
                    cell = new object[] {
                        q.groupname,
                        "<a href='#' class='btCar' data-rejId=" + q.qc.qc_reject_id + ">" + q.qc.Car.report_no + "</a>",
                        q.qc.Master_Product.product_name,
                        q.qc.item_code,
                        q.qc.problem,
                        q.qc.defective_lv,
                        q.qc.picture != null ? "<a class='fancybox' href='" + Url.Content("~/" + q.qc.picture) + "'><img src='" + Url.Content("~/" + q.qc.picture) + "' class='gridImage' /></a>" : "<img src='" + Url.Content("~/" + "Images/noimage.jpg") + "' class='gridImage' />",
                        q.qc.entry_date
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        public ActionResult GridOverDue(string sidx, string sord, int page, int rows, string search, int plantid)
        {
            DateTime newDate = DateTime.Now.Date;
            var dbSel = from c in dbQIS.Car.Where(w => newDate > w.reply_due_date && w.QCReject.status_id == 4 
                        && w.reply_date == null).ToList() //&& c.QCReject.defective_lv != "Critical"
                        join t in dbTNC.tnc_group_master.ToList() on c.responsible equals t.id into j
                        from t in j.DefaultIfEmpty()
                        select new { groupname = t.group_name == null ? "N/A" : t.group_name, qc = c };
            if (plantid != 0)
            {
                dbSel = dbSel.Where(c => c.qc.QCReject.plant_id == plantid);
            }
            //dbSel = dbSel.OrderBy(c => c.reply_due_date);

            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbSel.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            // This is possible because I'm using the LINQ Dynamic Query Library
            var qcrejects = dbSel.AsQueryable()
                    .OrderBy(sidx + " " + sord)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize);

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = qcrejects.ToList().Select(q => new
                {
                    i = q.qc.qc_reject_id,
                    cell = new object[] {
                        q.groupname,
                        "<a href='#' class='btCar' data-rejId=" + q.qc.qc_reject_id + ">" + q.qc.report_no + "</a>",
                        q.qc.QCReject.Master_Product.product_name,
                        q.qc.QCReject.item_code,
                        q.qc.QCReject.problem,
                        q.qc.QCReject.defective_lv,
                        q.qc.QCReject.picture != null ? "<a class='fancybox' href='" + Url.Content("~/" + q.qc.QCReject.picture) + "'><img src='" + Url.Content("~/" + q.qc.QCReject.picture) + "' class='gridImage' /></a>" : "<img src='" + Url.Content("~/" + "Images/noimage.jpg") + "' class='gridImage' />",
                        q.qc.reply_due_date,
                        newDate.Subtract(q.qc.reply_due_date).Days
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        public ActionResult GetShowCar(int id)
        {
            var cars = (from c in dbQIS.Car.ToList()
                        join t in dbTNC.tnc_group_master.ToList() on c.responsible equals t.id into j
                        from t in j.DefaultIfEmpty()
                        where c.qc_reject_id == id
                        select new
                        {
                            c.report_no,
                            c.found,
                            c.root_cause,
                            t.group_name,
                            c.qty_product,
                            c.qty_reject,
                            c.ipqc,
                            issued_date = c.issued_date.Day + "/" + c.issued_date.Month + "/" + c.issued_date.Year,
                            reply_due_date = c.reply_due_date.Day + "/" + c.reply_due_date.Month + "/" + c.reply_due_date.Year,
                            reply_date = c.reply_date == null ? "-" : c.reply_date.Value.Day + "/" + c.reply_date.Value.Month + "/" + c.reply_date.Value.Year,
                            respond_date = c.respond_date == null ? "-" : c.respond_date.Value.Day + "/" + c.respond_date.Value.Month + "/" + c.respond_date.Value.Year,
                            effective_date = c.effective_date == null ? "-" : c.effective_date.Value.Day + "/" + c.effective_date.Value.Month + "/" + c.effective_date.Value.Year,
                            c.attach
                        }).FirstOrDefault();
            if (cars != null)
                return Json(cars, JsonRequestBehavior.AllowGet);
            else
                return Json(0, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult GetShowMessage()
        {
            var msg = "";
            if (Session["QIS_Auth"] != null && Session["UserType"] != null)
            {
                var utype = Session["UserType"].ToString();
                var user = Session["QIS_Auth"].ToString();
                var plantid = int.Parse(Session["PlantID"].ToString());
                if (utype == "2")//Staff
                {
                    var query = dbQIS.QCReject.Where(q => q.status_id == 3 && q.user_code == user);
                    msg += "You have QC Reject which were rejected. [" + query.Count() + "]";
                }
                else if (utype == "3")//Manager
                {
                    var query = dbQIS.QCReject.Where(q => q.status_id == 2 && q.plant_id == plantid);
                    msg += "You have QC Reject waiting for approval. [" + query.Count() + "]";
                }
            }
            
            if (msg != "")
                return Json(msg, JsonRequestBehavior.AllowGet);
            else
                return Json(0, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetCriticalReject(GridSettings gridSettings)
        //{
        //    var data = dbQIS.QCReject.Where(w => w.defective_lv != null && w.defective_lv == "Critical").OrderBy(gridSettings.SortColumn + " " + gridSettings.SortOrder);

        //    // Count All Record
        //    double count = data.Count();

        //    // Prepare JSON Format
        //    object jsonData = new
        //    {
        //        page = gridSettings.PageIndex,
        //        total = Math.Ceiling(count / gridSettings.PageSize),
        //        records = count,
        //        rows = data.Select(d => new
        //        {
        //            d.Car.responsible,
        //            d.Car.report_no,
        //            d.Master_Product.product_name,
        //            d.item_code,
        //            d.problem,
        //            d.defective_lv,
        //            d.picture,
        //            d.Car.issued_date
        //        }).Skip((gridSettings.PageIndex - 1) * gridSettings.PageSize).Take(gridSettings.PageSize)
        //    };
        //    return Json(jsonData, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult GetCriticalReject(string sidx, string sord, int page, int rows, string search)
        {
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;

            var data = dbQIS.QCReject
                    .Where(w => w.defective_lv != null && w.defective_lv == "Critical");

            var qcrejects = data
                    .OrderBy(sidx + " " + sord)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize).AsEnumerable();

            var totalRecords = data.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = qcrejects.Select(q => new
                {
                    i = q.qc_reject_id,
                    cell = new object[] {
                        q.Car.responsible,
                        q.Car.report_no,
                        q.Master_Product.product_name,
                        q.item_code,
                        q.problem,
                        q.defective_lv,
                        q.picture,
                        q.Car.issued_date
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult GetMsg()
        {
            var user = Session["QIS_Auth"].ToString();
            var msg = (from m in dbQIS.Message
                       where m.user_code == user
                       select new
                       {
                           message = m.Master_Message.msg_word,
                           amount = m.count,
                           mid = m.msg_id
                       });

            if (msg != null)
                return Json(msg, JsonRequestBehavior.AllowGet);
            else
                return Json(0, JsonRequestBehavior.AllowGet);
        }
    }
}
