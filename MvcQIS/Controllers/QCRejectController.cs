using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using MvcQIS.Models;
using System.IO;
using System.Globalization;

namespace MvcQIS.Controllers
{
    public class QCRejectController : Controller
    {
        private QISEntities dbQIS = new QISEntities();
        private TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();
        private TNC_CenterEntities dbCenter = new TNC_CenterEntities();

        //
        // GET: /QCReject/
        public ActionResult Index()
        {
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (Session["UserType"].ToString() != "2" && Session["UserType"].ToString() != "0")// 2 = Staff
                    return RedirectToAction("Index", "Home");
            }
            var plant_id = int.Parse(Session["PlantID"].ToString());
            return View(dbQIS.Main.Where(m =>
                m.plant_id == plant_id).OrderByDescending(m => m.entry_date));
        }

        public ActionResult Approve()
        {
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (Session["UserType"].ToString() != "3" && Session["UserType"].ToString() != "4")// 3 = Manager
                    return RedirectToAction("Index", "Home");
            }
            ViewBag.Message = "Approval";
            
            var plantid = int.Parse(Session["PlantID"].ToString());
            var approve_date = (from q in dbQIS.QCReject
                                where (q.plant_id == plantid) && q.status_id == 2//Status:Wait Mgr. Approve
                                group q by q.entry_date into g
                                orderby g.Key descending
                                select new { entry_date = g.Key });
            List<string> lstDT = new List<string>();
            foreach (var item in approve_date)
            {
                lstDT.Add(DateToString(item.entry_date,"/"));
            }
            ViewBag.SelectDate = lstDT;

            return View();
        }

        public ActionResult Create(string dd, string mm, string yy)
        {
            ViewBag.Title = "Input Data";
            if (Session["QIS_Auth"] == null || Session["PlantID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (Session["UserType"].ToString() != "2" && Session["UserType"].ToString() != "0")// 2 = Staff
                    return RedirectToAction("Index", "Home");
            }
            var plantid = int.Parse(Session["PlantID"].ToString());
            DateTime dt = new DateTime();

            if (dd != null && mm != null & yy != null)
            {
                dt = DateTime.Parse(yy + "-" + mm + "-" + dd);
                if (dd.Length != 2) dd = "0" + dd;
                if (mm.Length != 2) mm = "0" + mm;
                ViewBag.InitialDate = dd + "-" + mm + "-" + yy;
            }
            var prd_mas = (from p in dbQIS.Master_Product
                           where p.plant_id == plantid && p.product_del_flag == false
                           select p);
            ViewBag.Product = prd_mas;

            var group_mas = (from g in dbTNC.tnc_group_master
                             orderby g.group_name
                             select g);
            ViewBag.Group = group_mas;
            //var all_user = (from u in dbTNC.tnc_user
            //                where u.emp_status == "A"
            //                orderby u.emp_fname, u.emp_lname
            //                select u).AsEnumerable();
            //ViewBag.AllUser = all_user;
            return View();
        }

        public ActionResult Reject()
        {
            ViewBag.Title = "Reject";
            var plantid = int.Parse(Session["PlantID"].ToString());
            var prd_mas = (from p in dbQIS.Master_Product
                           where p.plant_id == plantid && p.product_del_flag == false
                           select p);
            ViewBag.Product = prd_mas;
            return View();
        }

        public ActionResult CAR_Followup()
        {
            ViewBag.Title = "CAR Follow Up";
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (Session["UserType"].ToString() != "2" && Session["UserType"].ToString() != "0")// 2 = Staff
                    return RedirectToAction("Index", "Home");
            }
            var plantid = int.Parse(Session["PlantID"].ToString());
            var prd_mas = (from p in dbQIS.Master_Product
                           where p.plant_id == plantid && p.product_del_flag == false
                           select p);
            ViewBag.Product = prd_mas;

            var group_mas = (from g in dbTNC.tnc_group_master
                             orderby g.group_name
                             select g);
            ViewBag.Group = group_mas;
            return View();
        }

        public ActionResult MyReject()
        {
            ViewBag.Title = "Reject";
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (Session["UserType"].ToString() != "2" && Session["UserType"].ToString() != "0")// 2 = Staff
                    return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public ActionResult Detail(int id=0)
        {
            ViewBag.Title = "Detail";
            var qcreject = (from q in dbQIS.QCReject
                           where q.qc_reject_id == id
                           select q).FirstOrDefault();
            if (qcreject == null)
            {
                return HttpNotFound();
            }
            return View(qcreject);
        }

        public ActionResult MgrEdit()
        {
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (Session["UserType"].ToString() != "3")// 3 = Manager
                    return RedirectToAction("Index", "Home");
            }
            ViewBag.Title = "Mgr. Edit";
            var plantid = int.Parse(Session["PlantID"].ToString());
            var prd_mas = (from p in dbQIS.Master_Product
                           where p.plant_id == plantid && p.product_del_flag == false
                           select p);
            ViewBag.Product = prd_mas;

            var group_mas = (from g in dbTNC.tnc_group_master
                             orderby g.group_name
                             select g);
            ViewBag.Group = group_mas;

            return View();
        }

        //
        // GET: /QCReject/Create
        //public ActionResult Create()
        //{
        //    if (Session["QIS_Auth"] == null)
        //    {
        //        RedirectToAction("Login", "Account");
        //    }
        //    if (Session["PlantID"] == null)
        //    {
        //        RedirectToAction("Index", "Home");
        //    }
        //    var plantid = int.Parse(Session["PlantID"].ToString());

        //    var prd_mas = (from p in dbQIS.Master_Product
        //                   where p.plant_id == plantid
        //                   select p);

        //    var master = (from m in dbQIS.Main
        //                  where m.plant_id == plantid //&& m.entry_date == DateTime.Today
        //                  select m);

        //    //var inspec = (from i in dbQIS.InspectionLot
        //    //              where i.plant_id == plantid //&& i.entry_date == DateTime.Today
        //    //              select i);

        //    ViewBag.Product = prd_mas;
        //    //ViewBag.Inspec = inspec;
        //    //CheckDate(DateTime.Now.AddDays(-1).Date.ToString());
        //    return View(master);
        //}

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public JsonResult GetInspection(string mdate)
        {
            DateTime entrydate = ParseToDate(mdate);//mdate format dd-mm-yyyy
            var plantid = int.Parse(Session["PlantID"].ToString());
            var inspection = (from i in dbQIS.InspectionLot
                              where i.entry_date == entrydate && i.plant_id == plantid
                              select new
                              {
                                  prdid = i.product_id,
                                  qty = i.qty
                              });
            if (inspection != null)
                return Json(inspection, JsonRequestBehavior.AllowGet);
            else
                return Json(0, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public JsonResult GetQCReject(int id)
        {
            var qcreject = (from q in dbQIS.QCReject.ToList()
                            where q.qc_reject_id == id
                            select new
                            {
                                qc_reject_id = q.qc_reject_id,
                                product_id = q.product_id,
                                item_code = q.item_code,
                                lot_no = q.lot_no,
                                problem = q.problem,
                                def_lv = q.defective_lv,
                                shift = q.shift,
                                picture = q.picture,
                                //inspector1 = q.inspector,//Delete when real use.
                                //screener1 = q.screener,//Delete when real use.
                                issue_car = q.issue_car,
                                entrydate = DateToString(q.entry_date,"-")//q.entry_date.Day + "-" + q.entry_date.Month + "-" + q.entry_date.Year
                            }).FirstOrDefault();
            if (qcreject != null)
                return Json(qcreject, JsonRequestBehavior.AllowGet);
            else
                return Json(0, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult GetCar(int id)
        {
            var cars = (from c in dbQIS.Car.ToList()
                        where c.qc_reject_id == id
                        select new
                        {
                            c.qc_reject_id,
                            c.report_no,
                            c.found,
                            c.root_cause,
                            c.responsible,
                            c.qty_product,
                            c.qty_reject,
                            c.ipqc,
                            issued_date = c.issued_date.Day + "-" + c.issued_date.Month + "-" + c.issued_date.Year,
                            reply_due_date = c.reply_due_date.Day + "-" + c.reply_due_date.Month + "-" + c.reply_due_date.Year,
                            reply_date = c.reply_date == null ? "" : c.reply_date.Value.Day + "-" + c.reply_date.Value.Month + "-" + c.reply_date.Value.Year,
                            respond_date = c.respond_date == null ? "" : c.respond_date.Value.Day + "-" + c.respond_date.Value.Month + "-" + c.respond_date.Value.Year,
                            effective_date = c.effective_date == null ? "" : c.effective_date.Value.Day + "-" + c.effective_date.Value.Month + "-" + c.effective_date.Value.Year,
                            c.attach
                        }).FirstOrDefault();
            if (cars != null)
                return Json(cars, JsonRequestBehavior.AllowGet);
            else
                return Json(0, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult GetQCInspector(int id)
        {
            var operate = (from o in dbQIS.OperateUser
                           where o.qc_reject_id == id && o.outype_id == 1
                           select o.emp_id).ToList();
            var empcode = dbCenter.Employee.Where(w => operate.Contains(w.EmpId))
                .Select(s => new { id = s.EmpId, text = s.FirstName + " " + s.LastName });
            if (empcode != null)
                return Json(empcode, JsonRequestBehavior.AllowGet);
            else
                return Json(0, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult GetSetupman(int id)
        {
            var operate = (from o in dbQIS.OperateUser
                           where o.qc_reject_id == id && o.outype_id == 2
                           select o.emp_id).ToList();
            var empcode = dbCenter.Employee.Where(w => operate.Contains(w.EmpId))
                .Select(s => new { id = s.EmpId, text = s.FirstName + " " + s.LastName });
            if (empcode != null)
                return Json(empcode, JsonRequestBehavior.AllowGet);
            else
                return Json(0, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult GetScreener(int id)
        {
            var operate = (from o in dbQIS.OperateUser
                           where o.qc_reject_id == id && o.outype_id == 3
                           select o.emp_id).ToList();
            var empcode = dbCenter.Employee.Where(w => operate.Contains(w.EmpId))
                .Select(s => new { id = s.EmpId, text = s.FirstName + " " + s.LastName });
            if (empcode != null)
                return Json(empcode, JsonRequestBehavior.AllowGet);
            else
                return Json(0, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult GetQCRejectByDate(int d, int m, int y, int product_id)
        {
            string table = string.Empty;
            var query = from q in dbQIS.QCReject
                        where q.product_id == product_id && q.entry_date.Day == d
                        && q.entry_date.Month == m && q.entry_date.Year == y
                        select q;
            table = "<table class='tablesorter' cellspacing='1'><thead><tr><th>item</th><th>Problem</th></tr></thead><tbody>";
            foreach (var item in query)
            {
                table += "<tr><td>" + item.item_code + "</td><td>" + item.problem + "</td></tr>";
            }
            table += "</tbody></table>";

            return Json(table, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public JsonResult GetSummary(string mdate)
        {
            DateTime entrydate = ParseToDate(mdate);//mdate format dd-mm-yyyy

            var mm = entrydate.Month;
            var yy = entrydate.Year;
            var plantid = int.Parse(Session["PlantID"].ToString());
            
            var prd = from p in dbQIS.Master_Product
                      where p.plant_id == plantid && p.product_del_flag == false
                      select p;

            DateTime newEntryDate = entrydate.AddMonths(1);
            DateTime startOfMonth = new DateTime(newEntryDate.Year, newEntryDate.Month, 1);
            var endOfMonth = startOfMonth.AddDays(-1);
            
            string table = string.Empty, total = string.Empty, ng = string.Empty, rate = string.Empty, tbTotal = string.Empty, tbNG = string.Empty, tbRate = string.Empty;

            int[] totalSum = new int[endOfMonth.Day + 1];
            int[] totalNG = new int[endOfMonth.Day + 1];

            table += "<table id='tbSummary' class='bordered'>";
            //-------------Loop date----------------//
            table += "<tr><th>Line</th><th>Lot</th>";
            for (int i = 1; i <= endOfMonth.Day; i++)
            {
                table += "<th>" + i + "</th>";
            }
            table += "</tr>";
            //-------------Loop Products----------------//
            foreach (var item in prd)
            {
                total = "<th rowspan='3'>" + item.product_name + "</th>";
                
                total += "<th>Total</th>";
                ng = "<th>NG</th>";
                rate = "<th>Rate(%)</th>";

                for (int i = 1; i <= endOfMonth.Day; i++)
                {
                    var inspection = (from x in dbQIS.InspectionLot
                                      where x.plant_id == plantid && x.product_id == item.product_id
                                      && x.entry_date.Month == mm && x.entry_date.Year == yy
                                      && x.entry_date.Day == i
                                      orderby x.entry_date
                                      select x).FirstOrDefault();

                    if (inspection != null)
                    {
                        total += "<td>" + inspection.qty + "</td>";
                        totalSum[i] += inspection.qty;


                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date.Month == mm && q.entry_date.Year == yy
                                        && q.entry_date.Day == i && q.status_id == 4
                                        orderby q.entry_date
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            ng += "<td class='ngColor tooltipUI' data-date='" + i + "' data-month='" + mm + "' data-year='" + yy + "' data-product='" + item.product_id + "'>" + qcreject + "</td>";
                            totalNG[i] += qcreject;
                            double? per = 0.0;
                            if (inspection.qty != 0)
                                per = (qcreject * 100.0) / inspection.qty;

                            rate += "<td>" + string.Format("{0:#,0.00}", per) +"</td>";
                        }
                        else
                        {
                            ng += "<td>0</td>";
                            rate += "<td>0</td>";
                        }
                    }
                    else
                    {
                        total += "<td></td>";
                        ng += "<td></td>";
                        rate += "<td></td>";
                    }
                }
                table += "<tr>" + total + "</tr><tr>" + ng + "</tr><tr>" + rate + "</tr>";
            }

            tbTotal = "<th rowspan='3'>Summary</th><th>Total</th>";
            tbNG = "<th>NG</th>";
            tbRate = "<th>Rate(%)</th>";
            for (int i = 1; i <= endOfMonth.Day; i++)
            {
                tbTotal += "<td>" + totalSum[i] + "</td>";
                tbNG += "<td>" + totalNG[i] + "</td>";
                double sumRate = 0.0;
                if (totalSum[i] != 0 && totalNG[i] != 0)
                    sumRate = (totalNG[i] * 100.0) / totalSum[i];

                tbRate += "<td>" + string.Format("{0:#,0.##}", sumRate) + "</td>";
            }
            table += "<tr>" + tbTotal + "</tr><tr>" + tbNG + "</tr><tr>" + tbRate + "</tr>";
            table += "</table>";
            //table += "<p><input type='button' id='btnExport' value=' Export Table data into Excel ' /></p>";

            return Json(table, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteEntryDate(string mdate)
        {
            try
            {
                var entrydate = DateTime.Parse(Request.Form["mdate"].ToString());
                var plantid = int.Parse(Session["PlantID"] == null ? string.Empty : Session["PlantID"].ToString());

                var query = dbQIS.Main.Find(entrydate, plantid);
                dbQIS.Main.Remove(query);
                //var data = "entrydate=" + entrydate;// + "&plant=" + plantid;
                dbQIS.SaveChanges();
                return Json(1);
            }
            catch (Exception)
            {
                return Json(0);//Error : Delete QC Reject Unsuccessful.
            }
        }

        [HttpPost]
        public JsonResult AddInspection(string mdate, string inspec)
        {
            try
            {
                DateTime entrydate = ParseToDate(mdate);//mdate format dd-mm-yyyy
                var main = new Main();
                main.entry_date = entrydate;
                main.plant_id = int.Parse(Session["PlantID"].ToString());
                main.issue_dt = DateTime.Now;
                dbQIS.Main.Add(main);

                var a = inspec.Split('+');
                InspectionLot ins;
                foreach (var b in a)
                {
                    ins = new InspectionLot();
                    ins.entry_date = entrydate;
                    ins.plant_id = int.Parse(Session["PlantID"].ToString());
                    var c = b.Split(',');
                    ins.product_id = int.Parse(c[0]);
                    ins.qty = int.Parse(c[1]);
                    dbQIS.InspectionLot.Add(ins); 
                }
                dbQIS.SaveChanges();
                return Json("Add Inspection Successful.");
            }
            catch (Exception)
            {
                return Json("Error : Add Inspection Unsuccessful.");
            }
        }

        [HttpPost]
        public JsonResult UpdateInspection(string mdate, string inspec)
        {
            try
            {
                var plant = int.Parse(Session["PlantID"].ToString());
                var entrydate = ParseToDate(mdate);//mdate format dd-mm-yyyy

                var a = inspec.Split('+');
                InspectionLot ins;
                foreach (var b in a)
                {
                    var c = b.Split(',');
                    var product = int.Parse(c[0]);
                    var query = from i in dbQIS.InspectionLot
                                where i.entry_date.Equals(entrydate) && i.plant_id.Equals(plant) && i.product_id.Equals(product)
                                select i;
                    ins = query.First();
                    ins.qty = int.Parse(c[1]);
                }
                dbQIS.SaveChanges();
                return Json("Update Inspection Successful.");
            }
            catch (Exception)
            {
                return Json("Error : Update Inspection Unsuccessful.");
            }
        }

        [HttpPost]
        public ActionResult AddOperator(int id, string emp_code, DateTime entry_date, byte usertype, int plant_id)
        {
            try
            {
                var tdOperator = new OperateUser();
                tdOperator.qc_reject_id = id;
                tdOperator.emp_id = emp_code;
                tdOperator.entry_date = entry_date;
                tdOperator.outype_id = usertype;
                tdOperator.plant_id = plant_id;
                dbQIS.OperateUser.Add(tdOperator);
                dbQIS.SaveChanges();
                return Json(1);
            }
            catch (Exception)
            {
                return Json(0);
            }
        }
        
        public void DeleteOperator(int id, int utype)
        {
            try
            {
                var del_operator = dbQIS.OperateUser.Where(o => o.qc_reject_id == id && o.outype_id == utype);
                foreach (var item in del_operator)
                {
                    dbQIS.OperateUser.Remove(item);
                }
                dbQIS.SaveChanges();
            }
            catch (Exception)
            {
                
            }
        }

        [HttpPost]
        public JsonResult AddQCReject()
        {
            try
            {
                var inputDate = Request.Form["mdate"].ToString();
                var entrydate = ParseToDate(inputDate); //DateTime.Parse(Request.Form["mdate"].ToString());
                var plantid = int.Parse(Session["PlantID"] == null ? string.Empty : Session["PlantID"].ToString());
                
                var tblQCReject = new QCReject();
                tblQCReject.entry_date = entrydate;
                tblQCReject.plant_id = plantid;
                tblQCReject.product_id = int.Parse(Request.Form["product"].ToString());
                tblQCReject.item_code = Request.Form["itemcode"] == null ? null : Request.Form["itemcode"].ToString();
                tblQCReject.lot_no = Request.Form["lotno"] == null ? null : Request.Form["lotno"].ToString();
                tblQCReject.problem = Request.Form["problem"] == null ? string.Empty : Request.Form["problem"].ToString();
                tblQCReject.defective_lv = Request.Form["deflv"].ToString();
                tblQCReject.picture = Request.Form["picture"] == string.Empty ? null : Request.Form["picture"].ToString();
                //tblQCReject.inspector = Request.Form["inspector1"] == null ? string.Empty : Request.Form["inspector1"].ToString();//Delete when real use.
                //tblQCReject.screener = Request.Form["screener1"] == null ? string.Empty : Request.Form["screener1"].ToString();//Delete when real use.
                tblQCReject.shift = Request.Form["shift"].ToString();
                tblQCReject.issue_car = byte.Parse(Request.Form["car"].ToString());
                tblQCReject.status_id = 1;//Status Wait Mgr. Approve
                tblQCReject.issue_dt = DateTime.Now;
                tblQCReject.user_code = Session["QIS_Auth"].ToString();
                dbQIS.QCReject.Add(tblQCReject);
                
                if (tblQCReject.issue_car > 0)
                {
                    var tblCar = new Car();
                    tblCar.entry_date = entrydate;
                    tblCar.plant_id = plantid;
                    tblCar.qc_reject_id = tblQCReject.qc_reject_id;
                    tblCar.report_no = Request.Form["reportno"] == null ? string.Empty : Request.Form["reportno"].ToString();
                    tblCar.found = Request.Form["foundat"] == null ? string.Empty : Request.Form["foundat"].ToString();
                    tblCar.root_cause = Request.Form["root"].ToString();
                    tblCar.responsible = int.Parse(Request.Form["product2"].ToString());
                    tblCar.qty_product = int.Parse(Request.Form["qtyprd"].ToString());
                    tblCar.qty_reject = int.Parse(Request.Form["qtyrej"].ToString());
                    tblCar.ipqc = Request.Form["ipqc"].ToString();
                    tblCar.issued_date = ParseToDate(Request.Form["dateissue"].ToString());//DateTime.Parse(Request.Form["dateissue"].ToString());
                    tblCar.reply_due_date = ParseToDate(Request.Form["datedue"].ToString());//DateTime.Parse(Request.Form["datedue"].ToString());

                    if (Request.Form["datereply"] == string.Empty)
                        tblCar.reply_date = null;
                    else
                        tblCar.reply_date = ParseToDate(Request.Form["datereply"].ToString()); //DateTime.Parse(Request.Form["datereply"].ToString());

                    if (Request.Form["daterespond"] == string.Empty)
                        tblCar.respond_date = null;
                    else
                        tblCar.respond_date = ParseToDate(Request.Form["daterespond"].ToString()); //DateTime.Parse(Request.Form["daterespond"].ToString());

                    if (Request.Form["dateefft"] == string.Empty)
                        tblCar.effective_date = null;
                    else
                        tblCar.effective_date = ParseToDate(Request.Form["dateefft"].ToString()); //DateTime.Parse(Request.Form["dateefft"].ToString());

                    tblCar.attach = Request.Form["file"] == string.Empty ? null : Request.Form["file"].ToString();
                    tblCar.issue_dt = DateTime.Now;
                    dbQIS.Car.Add(tblCar);
                }

                if (Request.Form["inspector"] != null && Request.Form["inspector"].ToString() != string.Empty)
                {
                    var list_inspector = Request.Form["inspector"].ToString();
                    var insp = list_inspector.Split(',');
                    foreach (var item in insp)
                    {
                        AddOperator(tblQCReject.qc_reject_id, item, entrydate, 1, plantid);
                    }
                }

                if (Request.Form["setupman"] != null && Request.Form["setupman"].ToString() != string.Empty)
                {
                    var list_setupman = Request.Form["setupman"].ToString();
                    var setup = list_setupman.Split(',');
                    foreach (var item in setup)
                    {
                        AddOperator(tblQCReject.qc_reject_id, item, entrydate, 2, plantid);
                    }
                }

                if (Request.Form["screener"] != null && Request.Form["screener"].ToString() != string.Empty)
                {
                    var list_screener = Request.Form["screener"].ToString();
                    var screen = list_screener.Split(',');
                    foreach (var item in screen)
                    {
                        AddOperator(tblQCReject.qc_reject_id, item, entrydate, 3, plantid);
                    }
                }

                var data = "entrydate=" + inputDate;// + "&plant=" + plantid;
                dbQIS.SaveChanges();
                return Json(data);
            }
            catch (Exception)
            {
                return Json("entrydate=01/01/0001");//Error : Add QC Reject.
            }
        }

        [HttpPost]
        public JsonResult UpdateQCReject()
        {
            try
            {
                var id = int.Parse(Request.Form["reject_id"].ToString());
                var inputDate = Request.Form["mdate"].ToString();
                var entrydate = ParseToDate(inputDate); //DateTime.Parse(Request.Form["mdate"].ToString());
                var plantid = int.Parse(Session["PlantID"] == null ? string.Empty : Session["PlantID"].ToString());
                var query = (from q in dbQIS.QCReject
                             where q.entry_date.Equals(entrydate) && q.plant_id.Equals(plantid) && q.qc_reject_id.Equals(id)
                             select q).First();
                var issuer = query.user_code;
                query.product_id = int.Parse(Request.Form["product"].ToString());
                query.item_code = Request.Form["itemcode"] == null ? null : Request.Form["itemcode"].ToString();
                query.lot_no = Request.Form["lotno"] == null ? null : Request.Form["lotno"].ToString();
                query.problem = Request.Form["problem"] == null ? string.Empty : Request.Form["problem"].ToString();
                query.defective_lv = Request.Form["deflv"].ToString();
                query.shift = Request.Form["shift"].ToString();
                //query.inspector = Request.Form["inspector1"] == null ? string.Empty : Request.Form["inspector1"].ToString();//Delete when real use.
                //query.screener = Request.Form["screener1"] == null ? string.Empty : Request.Form["screener1"].ToString();//Delete when real use.
                query.issue_car = byte.Parse(Request.Form["car"].ToString());
                if (Request.Form["picture"] != string.Empty)
                    query.picture = Request.Form["picture"].ToString();

                if (query.status_id == 3)//for Change Status Rejected to Created 2013-04-06
                {
                    query.status_id = 1;
                    DecreaseMessage(issuer, 2);
                }

                if (query.issue_car != 0)//issue CAR
                {
                    var query_car = (from c in dbQIS.Car
                                     where c.entry_date.Equals(entrydate) && c.plant_id.Equals(plantid) && c.qc_reject_id.Equals(id)
                                     select c);

                    if (query_car.Count() == 0)//Add
                    {
                        var tblCar = new Car();
                        tblCar.report_no = Request.Form["reportno"] == null ? string.Empty : Request.Form["reportno"].ToString();
                        tblCar.found = Request.Form["foundat"] == null ? string.Empty : Request.Form["foundat"].ToString();
                        tblCar.root_cause = Request.Form["root"].ToString();
                        tblCar.responsible = int.Parse(Request.Form["product2"].ToString());
                        tblCar.qty_product = int.Parse(Request.Form["qtyprd"].ToString());
                        tblCar.qty_reject = int.Parse(Request.Form["qtyrej"].ToString());
                        tblCar.ipqc = Request.Form["ipqc"].ToString();
                        tblCar.issued_date = ParseToDate(Request.Form["dateissue"].ToString()); //DateTime.Parse(Request.Form["dateissue"].ToString());
                        tblCar.reply_due_date = ParseToDate(Request.Form["datedue"].ToString()); //DateTime.Parse(Request.Form["datedue"].ToString());

                        if (Request.Form["datereply"] == string.Empty)
                            tblCar.reply_date = null;
                        else
                            tblCar.reply_date = ParseToDate(Request.Form["datereply"].ToString()); //DateTime.Parse(Request.Form["datereply"].ToString());

                        if (Request.Form["daterespond"] == string.Empty)
                            tblCar.respond_date = null;
                        else
                            tblCar.respond_date = ParseToDate(Request.Form["daterespond"].ToString()); //DateTime.Parse(Request.Form["daterespond"].ToString());

                        if (Request.Form["dateefft"] == string.Empty)
                            tblCar.effective_date = null;
                        else
                            tblCar.effective_date = ParseToDate(Request.Form["dateefft"].ToString()); //DateTime.Parse(Request.Form["dateefft"].ToString());

                        if (Request.Form["file"] != string.Empty)
                            tblCar.attach = Request.Form["file"].ToString();

                        tblCar.issue_dt = DateTime.Now;
                        tblCar.entry_date = entrydate;
                        tblCar.plant_id = plantid;
                        tblCar.qc_reject_id = query.qc_reject_id;
                        dbQIS.Car.Add(tblCar);
                    }
                    else if (query_car.Count() == 1)//Update
                    {
                        Car cars = query_car.First();
                        cars.report_no = Request.Form["reportno"] == null ? string.Empty : Request.Form["reportno"].ToString();
                        cars.found = Request.Form["foundat"] == null ? string.Empty : Request.Form["foundat"].ToString();
                        cars.root_cause = Request.Form["root"].ToString();
                        cars.responsible = int.Parse(Request.Form["product2"].ToString());
                        cars.qty_product = int.Parse(Request.Form["qtyprd"].ToString());
                        cars.qty_reject = int.Parse(Request.Form["qtyrej"].ToString());
                        cars.ipqc = Request.Form["ipqc"].ToString();
                        cars.reply_due_date = ParseToDate(Request.Form["datedue"].ToString()); //DateTime.Parse(Request.Form["datedue"].ToString());

                        if (Request.Form["datereply"] == string.Empty)
                            cars.reply_date = null;
                        else
                            cars.reply_date = ParseToDate(Request.Form["datereply"].ToString()); //DateTime.Parse(Request.Form["datereply"].ToString());

                        if (Request.Form["daterespond"] == string.Empty)
                            cars.respond_date = null;
                        else
                            cars.respond_date = ParseToDate(Request.Form["daterespond"].ToString()); //DateTime.Parse(Request.Form["daterespond"].ToString());

                        if (Request.Form["dateefft"] == string.Empty)
                            cars.effective_date = null;
                        else
                            cars.effective_date = ParseToDate(Request.Form["dateefft"].ToString()); //DateTime.Parse(Request.Form["dateefft"].ToString());

                        if (Request.Form["file"] != string.Empty)
                            cars.attach = Request.Form["file"].ToString();
                    }
                }
                else//no issue CAR
                {
                    var query_car = (from c in dbQIS.Car
                                     where c.entry_date.Equals(entrydate) && c.plant_id.Equals(plantid) && c.qc_reject_id.Equals(id)
                                     select c);
                    if (query_car.Count() != 0)
                    {
                        var query_car_remove = dbQIS.Car.Find(entrydate, plantid, id);
                        dbQIS.Car.Remove(query_car_remove);
                    }
                }

                if (Request.Form["inspector"] != null && Request.Form["inspector"].ToString() != string.Empty)
                {
                    DeleteOperator(query.qc_reject_id, 1);
                    var list_inspector = Request.Form["inspector"].ToString();
                    var insp = list_inspector.Split(',');
                    foreach (var item in insp)
                    {
                        AddOperator(query.qc_reject_id, item, entrydate, 1, plantid);
                    }
                }
                else
                    DeleteOperator(query.qc_reject_id, 1);

                if (Request.Form["setupman"] != null && Request.Form["setupman"].ToString() != string.Empty)
                {
                    DeleteOperator(query.qc_reject_id, 2);
                    var list_setupman = Request.Form["setupman"].ToString();
                    var setup = list_setupman.Split(',');
                    foreach (var item in setup)
                    {
                        AddOperator(query.qc_reject_id, item, entrydate, 2, plantid);
                    }
                }
                else
                    DeleteOperator(query.qc_reject_id, 2);

                if (Request.Form["screener"] != null && Request.Form["screener"].ToString() != string.Empty)
                {
                    DeleteOperator(query.qc_reject_id, 3);
                    var list_screener = Request.Form["screener"].ToString();
                    var screen = list_screener.Split(',');
                    foreach (var item in screen)
                    {
                        AddOperator(query.qc_reject_id, item, entrydate, 3, plantid);
                    }
                }
                else
                    DeleteOperator(query.qc_reject_id, 3);

                var data = "entrydate=" + inputDate;// + "&plant=" + plantid;
                dbQIS.SaveChanges();
                return Json(data);
            }
            catch (Exception)
            {
                return Json("entrydate=01/01/0001");//Error : Update QC Reject Unsuccessful.
            }
        }

        [HttpPost]
        public JsonResult UpdateQCRejectbyMgr()
        {
            try
            {
                var id = int.Parse(Request.Form["reject_id"].ToString());
                var inputDate = Request.Form["mdate"].ToString();
                var entrydate = ParseToDate(inputDate); //DateTime.Parse(Request.Form["mdate"].ToString());
                var plantid = int.Parse(Session["PlantID"] != null ? Session["PlantID"].ToString() : string.Empty);
                var query = (from q in dbQIS.QCReject
                             where q.entry_date.Equals(entrydate) && q.plant_id.Equals(plantid) && q.qc_reject_id.Equals(id)
                             select q).FirstOrDefault();
                var issuer = query.user_code;
                query.product_id = int.Parse(Request.Form["product"].ToString());
                query.item_code = Request.Form["itemcode"] == null ? null : Request.Form["itemcode"].ToString();
                query.lot_no = Request.Form["lotno"] == null ? null : Request.Form["lotno"].ToString();
                query.problem = Request.Form["problem"] == null ? string.Empty : Request.Form["problem"].ToString();
                query.defective_lv = Request.Form["deflv"].ToString();
                query.shift = Request.Form["shift"].ToString();
                //query.inspector = Request.Form["inspector1"] == null ? string.Empty : Request.Form["inspector1"].ToString();//Delete when real use.
                //query.screener = Request.Form["screener1"] == null ? string.Empty : Request.Form["screener1"].ToString();//Delete when real use.
                query.issue_car = byte.Parse(Request.Form["car"].ToString());
                if (Request.Form["picture"] != string.Empty)
                    query.picture = Request.Form["picture"].ToString();

                //if (query.status_id == 3)//for Change Status Rejected to Created 2013-04-06
                //{
                //    query.status_id = 1;
                //    DecreaseMessage(issuer, 2);
                //}

                if (query.issue_car != 0)//issue CAR
                {
                    var query_car = (from c in dbQIS.Car
                                     where c.entry_date.Equals(entrydate) && c.plant_id.Equals(plantid) && c.qc_reject_id.Equals(id)
                                     select c);

                    if (query_car.Count() == 0)//Add
                    {
                        var tblCar = new Car();
                        tblCar.report_no = Request.Form["reportno"] == null ? string.Empty : Request.Form["reportno"].ToString();
                        tblCar.found = Request.Form["foundat"] == null ? string.Empty : Request.Form["foundat"].ToString();
                        tblCar.root_cause = Request.Form["root"].ToString();
                        tblCar.responsible = int.Parse(Request.Form["product2"].ToString());
                        tblCar.qty_product = int.Parse(Request.Form["qtyprd"].ToString());
                        tblCar.qty_reject = int.Parse(Request.Form["qtyrej"].ToString());
                        tblCar.ipqc = Request.Form["ipqc"].ToString();
                        tblCar.issued_date = ParseToDate(Request.Form["dateissue"].ToString()); //DateTime.Parse(Request.Form["dateissue"].ToString());
                        tblCar.reply_due_date = ParseToDate(Request.Form["datedue"].ToString()); //DateTime.Parse(Request.Form["datedue"].ToString());

                        if (Request.Form["datereply"] == string.Empty)
                            tblCar.reply_date = null;
                        else
                            tblCar.reply_date = ParseToDate(Request.Form["datereply"].ToString()); //DateTime.Parse(Request.Form["datereply"].ToString());

                        if (Request.Form["daterespond"] == string.Empty)
                            tblCar.respond_date = null;
                        else
                            tblCar.respond_date = ParseToDate(Request.Form["daterespond"].ToString()); //DateTime.Parse(Request.Form["daterespond"].ToString());

                        if (Request.Form["dateefft"] == string.Empty)
                            tblCar.effective_date = null;
                        else
                            tblCar.effective_date = ParseToDate(Request.Form["dateefft"].ToString()); //DateTime.Parse(Request.Form["dateefft"].ToString());

                        if (Request.Form["file"] != string.Empty)
                            tblCar.attach = Request.Form["file"].ToString();

                        tblCar.issue_dt = DateTime.Now;
                        tblCar.entry_date = entrydate;
                        tblCar.plant_id = plantid;
                        tblCar.qc_reject_id = query.qc_reject_id;
                        dbQIS.Car.Add(tblCar);
                    }
                    else if (query_car.Count() == 1)//Update
                    {
                        Car cars = query_car.First();
                        cars.report_no = Request.Form["reportno"] == null ? string.Empty : Request.Form["reportno"].ToString();
                        cars.found = Request.Form["foundat"] == null ? string.Empty : Request.Form["foundat"].ToString();
                        cars.root_cause = Request.Form["root"].ToString();
                        cars.responsible = int.Parse(Request.Form["product2"].ToString());
                        cars.qty_product = int.Parse(Request.Form["qtyprd"].ToString());
                        cars.qty_reject = int.Parse(Request.Form["qtyrej"].ToString());
                        cars.ipqc = Request.Form["ipqc"].ToString();
                        cars.reply_due_date = ParseToDate(Request.Form["datedue"].ToString()); //DateTime.Parse(Request.Form["datedue"].ToString());

                        if (Request.Form["datereply"] == string.Empty)
                            cars.reply_date = null;
                        else
                            cars.reply_date = ParseToDate(Request.Form["datereply"].ToString()); //DateTime.Parse(Request.Form["datereply"].ToString());

                        if (Request.Form["daterespond"] == string.Empty)
                            cars.respond_date = null;
                        else
                            cars.respond_date = ParseToDate(Request.Form["daterespond"].ToString()); //DateTime.Parse(Request.Form["daterespond"].ToString());

                        if (Request.Form["dateefft"] == string.Empty)
                            cars.effective_date = null;
                        else
                            cars.effective_date = ParseToDate(Request.Form["dateefft"].ToString()); //DateTime.Parse(Request.Form["dateefft"].ToString());

                        if (Request.Form["file"] != string.Empty)
                            cars.attach = Request.Form["file"].ToString();
                    }
                }
                else//no issue CAR
                {
                    var query_car = (from c in dbQIS.Car
                                     where c.entry_date.Equals(entrydate) && c.plant_id.Equals(plantid) && c.qc_reject_id.Equals(id)
                                     select c);
                    if (query_car.Count() != 0)
                    {
                        var query_car_remove = dbQIS.Car.Find(entrydate, plantid, id);
                        dbQIS.Car.Remove(query_car_remove);
                    }
                }

                if (Request.Form["inspector"] != null && Request.Form["inspector"].ToString() != string.Empty)
                {
                    DeleteOperator(query.qc_reject_id, 1);
                    var list_inspector = Request.Form["inspector"].ToString();
                    var insp = list_inspector.Split(',');
                    foreach (var item in insp)
                    {
                        AddOperator(query.qc_reject_id, item, entrydate, 1, plantid);
                    }
                }
                else
                    DeleteOperator(query.qc_reject_id, 1);

                if (Request.Form["setupman"] != null && Request.Form["setupman"].ToString() != string.Empty)
                {
                    DeleteOperator(query.qc_reject_id, 2);
                    var list_setupman = Request.Form["setupman"].ToString();
                    var setup = list_setupman.Split(',');
                    foreach (var item in setup)
                    {
                        AddOperator(query.qc_reject_id, item, entrydate, 2, plantid);
                    }
                }
                else
                    DeleteOperator(query.qc_reject_id, 2);

                if (Request.Form["screener"] != null && Request.Form["screener"].ToString() != string.Empty)
                {
                    DeleteOperator(query.qc_reject_id, 3);
                    var list_screener = Request.Form["screener"].ToString();
                    var screen = list_screener.Split(',');
                    foreach (var item in screen)
                    {
                        AddOperator(query.qc_reject_id, item, entrydate, 3, plantid);
                    }
                }
                else
                    DeleteOperator(query.qc_reject_id, 3);

                var data = "entrydate=" + inputDate;// + "&plant=" + plantid;
                dbQIS.SaveChanges();
                return Json(data);
            }
            catch (Exception)
            {
                return Json("entrydate=01/01/0001");//Error : Update QC Reject Unsuccessful.
            }
        }

        [HttpPost]
        public JsonResult DeleteQCReject()
        {
            try
            {
                var id = int.Parse(Request.Form["reject_id"].ToString());
                var inputDate = Request.Form["mdate"].ToString();
                var entrydate = ParseToDate(inputDate); //DateTime.Parse(Request.Form["mdate"].ToString());
                var plantid = int.Parse(Session["PlantID"] == null ? string.Empty : Session["PlantID"].ToString());

                //var query_qcrej = dbQIS.QCReject.Find(entrydate, plantid, id);
                //dbQIS.QCReject.Remove(query_qcrej);

                var query = (from q in dbQIS.QCReject
                             where q.entry_date.Equals(entrydate) && q.plant_id.Equals(plantid) && q.qc_reject_id.Equals(id)
                             select q).First();
                query.status_id = 9;

                var data = "entrydate=" + inputDate;// + "&plant=" + plantid;
                dbQIS.SaveChanges();
                return Json(data);
            }
            catch (Exception)
            {
                return Json("entrydate=01/01/0001");//Error : Delete QC Reject Unsuccessful.
            }
        }

        [HttpPost]
        public JsonResult CompleteQCReject()
        {
            try
            {
                var inputDate = Request.Form["mdate"].ToString();
                var entrydate = ParseToDate(inputDate); //DateTime.Parse(Request.Form["mdate"].ToString());
                var plantid = int.Parse(Session["PlantID"] == null ? string.Empty : Session["PlantID"].ToString());
                var user = Session["QIS_Auth"].ToString();
                var query = (from q in dbQIS.QCReject
                             where q.entry_date.Equals(entrydate) && q.plant_id.Equals(plantid) && q.status_id.Equals(1) && q.user_code.Equals(user)
                             select q);
                var count = 0;
                foreach (var item in query)
                {
                    item.status_id = 2;
                    count++;
                }

                var data = "entrydate=" + inputDate;// + "&plant=" + plantid;
                dbQIS.SaveChanges();

                AddMessage(GetMgrByPlant(plantid), 1, count);

                return Json(data);
            }
            catch (Exception)
            {
                return Json("entrydate=01/01/0001");
            }
        }

        public void AddMessage(string user, int msg_id, int add_count)
        {
            try
            {
                var selMsg = (from m in dbQIS.Message
                              where m.user_code == user && m.msg_id == msg_id
                              select m).FirstOrDefault();

                if (selMsg != null)
                {
                    //Update
                    selMsg.count += add_count;
                }
                else
                {
                    //Add
                    var tblMsg = new Message();
                    tblMsg.user_code = user;
                    tblMsg.msg_id = msg_id;
                    tblMsg.count = add_count;
                    dbQIS.Message.Add(tblMsg);
                }
                dbQIS.SaveChanges();
            }
            catch (Exception ex)
            {
                Response.Write("Error Add Message : " + ex);
            }
        }

        public void DecreaseMessage(string user, int msg_id)
        {
            try
            {
                var selMsg = (from m in dbQIS.Message
                              where m.user_code == user && m.msg_id == msg_id
                              select m);

                if (selMsg.Count() != 0)
                {
                    //Add
                    //var tblMsg = new Message();
                    //tblMsg.user_code = user;
                    //tblMsg.msg_id = msg_id;
                    //tblMsg.count = add_count;
                    //dbQIS.Message.Add(tblMsg);
                    var updMsg = selMsg.First();
                    updMsg.count -= 1;
                }
                //else
                //{
                //    var updMsg = selMsg.First();
                //    updMsg.count += add_count;
                //}
                dbQIS.SaveChanges();
            }
            catch (Exception ex)
            {
                Response.Write("Error Add Message : " + ex);
            }
        }

        public void DeleteMessage(string user, int msg_id)
        {
            try
            {
                var delMsg = dbQIS.Message.Find(user, msg_id);
                dbQIS.Message.Remove(delMsg);
                
                dbQIS.SaveChanges();
            }
            catch (Exception ex)
            {
                Response.Write("Error Delete Message : " + ex);
            }
        }

        [HttpPost]
        public JsonResult DeleteMessage()
        {
            var user = Session["QIS_Auth"].ToString();
            var msg_id = int.Parse(Request.Form["id"].ToString());
            try
            {
                var delMsg = dbQIS.Message.Find(user, msg_id);
                dbQIS.Message.Remove(delMsg);

                dbQIS.SaveChanges();
                return Json("1");
            }
            catch (Exception)
            {
                return Json("0");
            }
        }

        [HttpPost]
        public JsonResult ApproveQCReject()
        {
            try
            {
                var id = int.Parse(Request.Form["reject_id"].ToString());
                var inputDate = Request.Form["mdate"].ToString();
                var entrydate = ParseToDate(inputDate);
                var plantid = int.Parse(Session["PlantID"] == null ? string.Empty : Session["PlantID"].ToString());
                var approver = Session["QIS_Auth"] != null ? Session["QIS_Auth"].ToString() : string.Empty;
                
                var query = (from q in dbQIS.QCReject
                             where q.entry_date.Equals(entrydate) && q.plant_id.Equals(plantid) && q.qc_reject_id.Equals(id)
                             select q).First();
                query.status_id = 4;
                var deflv = query.defective_lv;

                var appv = new Approved_Log();
                appv.entry_date = entrydate;
                appv.plant_id = plantid;
                appv.qc_reject_id = id;
                appv.approved_by = approver;
                appv.approved_dt = DateTime.Now;
                dbQIS.Approved_Log.Add(appv);
                dbQIS.SaveChanges();

                string receiver = GetEmailQC(approver) + GetEmailProduction(query.Master_Product.group_id);
                if (receiver.Length > 0)
                    receiver = receiver.Substring(1);

                if (deflv == "Critical")
                {
                    InsertMailCenter(receiver, id);
                }
                else
                {
                    receiver += "," + GetEmailProdSupDown(query.Master_Product.group_id);
                    InsertTemp(receiver, id);
                }
                DecreaseMessage(GetMgrByPlant(plantid), 1);
                return Json(inputDate);
            }
            catch (Exception)
            {
                return Json("01/01/0001");//Error : Delete QC Reject Unsuccessful.
            }
        }

        [HttpPost]
        public JsonResult RejectQCReject()
        {
            try
            {
                var id = int.Parse(Request.Form["reject_id"].ToString());
                var inputDate = Request.Form["mdate"].ToString();
                var entrydate = ParseToDate(inputDate); //DateTime.Parse(Request.Form["mdate"].ToString());
                var reason = Request.Form["reason"].ToString();
                var plantid = int.Parse(Session["PlantID"] == null ? string.Empty : Session["PlantID"].ToString());

                var query = (from q in dbQIS.QCReject
                             where q.entry_date.Equals(entrydate) && q.plant_id.Equals(plantid) && q.qc_reject_id.Equals(id)
                             select q).First();
                query.status_id = 3;
                var issuer = query.user_code;
                var rej = new Rejected_Log();
                rej.entry_date = entrydate;
                rej.plant_id = plantid;
                rej.qc_reject_id = id;
                rej.rejected_by = Session["QIS_Auth"].ToString();
                rej.rejected_dt = DateTime.Now;
                rej.reason = reason;
                dbQIS.Rejected_Log.Add(rej);

                //var data = inputDate;// + "&plant=" + plantid;
                dbQIS.SaveChanges();
                AddMessage(issuer, 2, 1);
                DecreaseMessage(GetMgrByPlant(plantid), 1);
                return Json(inputDate);
            }
            catch (Exception)
            {
                return Json("01/01/0001");//Error : Delete QC Reject Unsuccessful.
            }
        }

        [HttpPost]
        public JsonResult RejectQCRejectbyMgr()
        {
            try
            {
                var id = int.Parse(Request.Form["reject_id"].ToString());
                //var inputDate = Request.Form["mdate"].ToString();
                //var entrydate = ParseToDate(inputDate); //DateTime.Parse(Request.Form["mdate"].ToString());
                var reason = Request.Form["reason"].ToString();
                var plantid = int.Parse(Session["PlantID"] == null ? string.Empty : Session["PlantID"].ToString());

                var query = (from q in dbQIS.QCReject
                             where q.plant_id.Equals(plantid) && q.qc_reject_id.Equals(id) //&& q.entry_date.Equals(entrydate)
                             select q).First();
                query.status_id = 3;
                var issuer = query.user_code;
                var rej = new Rejected_Log();
                rej.entry_date = query.entry_date;
                rej.plant_id = plantid;
                rej.qc_reject_id = id;
                rej.rejected_by = Session["QIS_Auth"].ToString();
                rej.rejected_dt = DateTime.Now;
                rej.reason = reason;
                dbQIS.Rejected_Log.Add(rej);

                //var data = inputDate;// + "&plant=" + plantid;
                dbQIS.SaveChanges();
                //AddMessage(issuer, 2, 1);
                //DecreaseMessage(GetMgrByPlant(plantid), 1);
                return Json("");
            }
            catch (Exception)
            {
                return Json("01/01/0001");//Error : Delete QC Reject Unsuccessful.
            }
        }

        [HttpPost]
        public JsonResult UpdateDate()
        {
            try
            {
                var id = int.Parse(Request.Form["reject_id"].ToString());
                var query_car = (from c in dbQIS.Car
                                 where c.qc_reject_id.Equals(id)
                                 select c);
                Car cars = query_car.First();
                if (Request.Form["datereply"] == string.Empty)
                    cars.reply_date = null;
                else
                    cars.reply_date = ParseToDate(Request.Form["datereply"].ToString());//DateTime.Parse(Request.Form["datereply"].ToString());

                if (Request.Form["daterespond"] == string.Empty)
                    cars.respond_date = null;
                else
                    cars.respond_date = ParseToDate(Request.Form["daterespond"].ToString());//DateTime.Parse(Request.Form["daterespond"].ToString());

                if (Request.Form["dateefft"] == string.Empty)
                    cars.effective_date = null;
                else
                    cars.effective_date = ParseToDate(Request.Form["dateefft"].ToString());//DateTime.Parse(Request.Form["dateefft"].ToString());

                if (Request.Form["file"] != string.Empty)
                    cars.attach = Request.Form["file"].ToString();

                dbQIS.SaveChanges();
                return Json(1);
            }
            catch (Exception)
            {
                return Json(0);//Error : Update QC Reject Unsuccessful.
            }
        }

        public ActionResult GridMainData(string sidx, string sord, int page, int rows, string search, string startdate, string enddate)
        {
            CultureInfo enUS = new CultureInfo("en-US");

            DateTime startValue, endValue;

            var plant = int.Parse(Session["PlantID"].ToString());
            var dbSel = dbQIS.Main.Where(u => u.plant_id == plant);
            if (DateTime.TryParseExact(startdate, "dd-MM-yyyy", enUS, DateTimeStyles.None, out startValue))
                dbSel = dbSel.Where(q => q.entry_date >= startValue);

            if (DateTime.TryParseExact(enddate, "dd-MM-yyyy", enUS, DateTimeStyles.None, out endValue))
                dbSel = dbSel.Where(q => q.entry_date <= endValue);

            dbSel = dbSel.OrderByDescending(u => u.entry_date);

            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbSel.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            // This is possible because I'm using the LINQ Dynamic Query Library
            var mains = dbSel.OrderBy(sidx + " " + sord)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize).AsEnumerable();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = mains.Select(m => new
                {
                    i = m.entry_date,
                    cell = new object[] {
                        m.entry_date,
                        m.Master_Plant.plant_name,
                        "<a href='" + Url.Action("Create","QCReject") 
                        + "?dd=" + m.entry_date.Day + "&mm=" + m.entry_date.Month + "&yy=" + m.entry_date.Year + "'><img src='" + Url.Content("~/" + "Images/edit_doc_24.png") + "' /></a>",
                        "<a href='#' class='btDel' data-Entry=" + m.entry_date + "><img src='" + Url.Content("~/" + "Images/del_doc_24.png") + "' /></a>"
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        public ActionResult GridAddQCReject(string sidx, string sord, int page, int rows, string search, string entrydate)
        {
            var plantid = int.Parse(Session["PlantID"].ToString());
            var user = Session["QIS_Auth"].ToString();
            var entry_date = ParseToDate(entrydate);//mdate format dd-mm-yyyy
            var dbSel = dbQIS.QCReject.Where(q => q.plant_id == plantid && q.entry_date == entry_date && q.user_code == user);
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbSel.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            // This is possible because I'm using the LINQ Dynamic Query Library
            var qcrejects = dbSel.OrderBy(sidx + " " + sord)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize).AsEnumerable();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = qcrejects.ToList().Select(m => new
                {
                    i = m.entry_date,
                    cell = new object[] {
                        m.Master_Product.product_name,
                        m.item_code,
                        m.problem,
                        m.lot_no,
                        m.defective_lv,
                        m.shift,
                        m.picture != null ? "<a class='fancybox' href='" + Url.Content("~/" + m.picture) + "'><img src='" + Url.Content("~/" + m.picture) + "' class='gridImage' /></a>" : "<img src='" + Url.Content("~/" + "Images/noimage.jpg") + "' class='gridImage' />",
                        m.issue_car == 1 ? "<a href='#' class='btCar' data-rejId=" + m.qc_reject_id + ">Yes</a>" : "No",
                        m.Master_Status.status_name,
                        m.Approved_Log != null ? GetEmpName(m.Approved_Log.approved_by) : "",
                        m.user_code == user && m.status_id < 4 ? "<a href='#' class='btEdit' data-rejId=" + m.qc_reject_id + "><img src='" + Url.Content("~/" + "Images/edit_doc_24.png") + "' /></a>" : "",
                        m.user_code == user && m.status_id < 4 ? "<a href='#' class='btDel' data-rejId=" + m.qc_reject_id + "><img src='" + Url.Content("~/" + "Images/del_doc_24.png") + "' /></a>" : ""
                        //Change status_id from < 3 to = 1 When Start Live
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        public ActionResult GridMgrEdit(string sidx, string sord, int page, int rows, string search)
        {
            var plantid = int.Parse(Session["PlantID"].ToString());
            var user = Session["QIS_Auth"].ToString();
            //var entry_date = ParseToDate(entrydate);//mdate format dd-mm-yyyy
            var dbSel = dbQIS.QCReject.Where(q => q.plant_id == plantid && q.status_id == 4);
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbSel.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            // This is possible because I'm using the LINQ Dynamic Query Library
            var qcrejects = dbSel.OrderBy(sidx + " " + sord)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize).AsEnumerable();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = qcrejects.ToList().Select(m => new
                {
                    i = m.entry_date,
                    cell = new object[] {
                        m.entry_date,
                        m.Master_Product.product_name,
                        m.item_code,
                        m.problem,
                        m.lot_no,
                        m.defective_lv,
                        m.shift,
                        m.picture != null ? "<a class='fancybox' href='" + Url.Content("~/" + m.picture) + "'><img src='" + Url.Content("~/" + m.picture) + "' class='gridImage' /></a>" : "<img src='" + Url.Content("~/" + "Images/noimage.jpg") + "' class='gridImage' />",
                        m.issue_car == 1 ? "<a href='#' class='btCar' data-rejId=" + m.qc_reject_id + ">Yes</a>" : "No",
                        //m.Master_Status.status_name,
                        GetEmpName(m.Approved_Log.approved_by),
                        "<a href='#' class='btEdit' data-rejId=" + m.qc_reject_id + "><img src='" + Url.Content("~/" + "Images/edit_doc_24.png") + "' /></a>",
                        "<a href='#' class='btRej' data-rejId=" + m.qc_reject_id + "><img src='" + Url.Content("~/" + "Images/Delete-icon24.png") + "' /></a>"
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        private string GetEmpName(string p)
        {
            var get_name = (from a in dbTNC.tnc_user
                            where a.emp_code == p
                            select a).FirstOrDefault();
            if (get_name != null)
            {
                return get_name.emp_fname + " " + get_name.emp_lname;
            }
            else
            {
                return "error get name";
            }
        }

        public ActionResult GridApprove(string sidx, string sord, int page, int rows, string search, string entrydate)
        {
            var plantid = int.Parse(Session["PlantID"].ToString());
            var user = Session["QIS_Auth"].ToString();
            var entry_date = ParseToDate(entrydate);
            var dbSel = dbQIS.QCReject.Where(q => q.plant_id == plantid && q.entry_date == entry_date && q.status_id == 2);
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbSel.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            // This is possible because I'm using the LINQ Dynamic Query Library
            var qcrejects = dbSel.OrderBy(sidx + " " + sord)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize).AsEnumerable();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = qcrejects.ToList().Select(m => new
                {
                    i = m.entry_date,
                    cell = new object[] {
                        "<a href='#' class='btAppv' data-rejId=" + m.qc_reject_id + "><img src='" + Url.Content("~/" + "Images/Check-icon24.png") + "' /></a>",
                        "<a href='#' class='btRej' data-rejId=" + m.qc_reject_id + "><img src='" + Url.Content("~/" + "Images/Delete-icon24.png") + "' /></a>",
                        m.Master_Product.product_name,
                        m.item_code,
                        m.lot_no,
                        m.problem,
                        m.defective_lv,
                        m.shift,
                        m.picture != null ? "<a class='fancybox' href='" + Url.Content("~/" + m.picture) + "'><img src='" + Url.Content("~/" + m.picture) + "' class='gridImage' /></a>" : "<img src='" + Url.Content("~/" + "Images/noimage.jpg") + "' class='gridImage' />",
                        //m.inspector,
                        //m.screener,
                        m.issue_car == 1 ? "<a href='#' class='btCar' data-rejId=" + m.qc_reject_id + ">Yes</a>" : "No"
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        public ActionResult GridCARFollowup(string sidx, string sord, int page, int rows, string search)
        {
            //var user = Session["QIS_Auth"].ToString();
            var plantid = int.Parse(Session["PlantID"].ToString());

            var dbSel = from d in dbQIS.QCReject.Where(w => w.issue_car == 1
                        && (w.Car.reply_date == null || w.Car.respond_date == null || w.Car.effective_date == null) 
                        && w.plant_id == plantid).ToList() //&& d.user_code == user
                        join t in dbTNC.tnc_group_master.ToList() on d.Car.responsible equals t.id into j
                        from t in j.DefaultIfEmpty()
                        select new { groupname = t.group_name == null ? "N/A" : t.group_name, qc = d };
            
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
                        q.qc.Car.report_no,
                        //"<a href='#' class='btCar' data-rejId=" + q.qc_reject_id + ">" + q.Car.report_no + "</a>",
                        q.qc.Master_Product.product_name,
                        q.qc.item_code,
                        q.qc.problem,
                        q.qc.defective_lv,
                        q.qc.Car.reply_due_date,
                        "<a href='#' class='btEdit' data-rejId=" + q.qc.qc_reject_id + "><img src='" + Url.Content("~/" + "Images/edit_doc_24.png") + "' /></a>"
                    }
                }).ToArray()
            };
            return Json(jsonData);
        }

        public ActionResult GridReject(string sidx, string sord, int page, int rows, string search)
        {
            var plantid = int.Parse(Session["PlantID"].ToString());
            var user = Session["QIS_Auth"].ToString();
            //var dbSel = dbQIS.Main.Where(u => u.plant_id == plant).OrderByDescending(u => u.entry_date);
            var dbSel = dbQIS.QCReject.Where(q => q.plant_id == plantid && q.status_id == 3);//status_id = 3 is Mgr. Rejected
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbSel.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            // This is possible because I'm using the LINQ Dynamic Query Library
            var qcrejects = dbSel.OrderBy(sidx + " " + sord)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize).AsEnumerable();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = qcrejects.ToList().Select(m => new
                {
                    i = m.entry_date,
                    cell = new object[] {
                        m.Master_Product.product_name,
                        m.item_code,
                        m.lot_no,
                        m.defective_lv,
                        m.shift,
                        m.picture != null ? "<a class='fancybox' href='" + Url.Content("~/" + m.picture) + "'><img src='" + Url.Content("~/" + m.picture) + "' class='gridImage' /></a>" : "<img src='" + Url.Content("~/" + "Images/noimage.jpg") + "' class='gridImage' />",
                        m.inspector,
                        m.screener,
                        m.issue_car == 1 ? "<a href='#' class='btCar' data-rejId=" + m.qc_reject_id + ">Yes</a>" : "No",
                        m.Master_Status.status_name,
                        m.status_id ==3 ? "<a href='#' class='btEdit' data-rejId=" + m.qc_reject_id + "><img src='" + Url.Content("~/" + "Images/edit_doc_24.png") + "' /></a>" : ""
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        public ActionResult GridMyReject(string sidx, string sord, int page, int rows, string search)
        {
            var plantid = int.Parse(Session["PlantID"].ToString());
            var user = Session["QIS_Auth"].ToString();
            var dbSel = dbQIS.QCReject.Where(q => q.plant_id == plantid && q.status_id == 3 && q.user_code == user);//status_id = 3 is Mgr. Rejected
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbSel.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            // This is possible because I'm using the LINQ Dynamic Query Library
            var qcrejects = dbSel.OrderBy(sidx + " " + sord)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize).AsEnumerable();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = qcrejects.ToList().Select(m => new
                {
                    i = m.entry_date,
                    cell = new object[] {
                        m.Master_Product.product_name,
                        m.item_code,
                        m.lot_no,
                        m.defective_lv,
                        m.shift,
                        m.picture != null ? "<a class='fancybox' href='" + Url.Content("~/" + m.picture) + "'><img src='" + Url.Content("~/" + m.picture) + "' class='gridImage' /></a>" : "<img src='" + Url.Content("~/" + "Images/noimage.jpg") + "' class='gridImage' />",
                        //m.issue_car == 1 ? "<a href='#' class='btCar' data-rejId=" + m.qc_reject_id + ">Yes</a>" : "No",
                        m.issue_car == 1 ? "Yes" : "No",
                        m.Master_Status.status_name,
                        m.entry_date,
                        "<a href='" + Url.Action("Create","QCReject") 
                        + "?dd=" + m.entry_date.Day + "&mm=" + m.entry_date.Month + "&yy=" + m.entry_date.Year + "'><img src='" + Url.Content("~/" + "Images/edit_doc_24.png") + "' /></a>"
                    }
                }).ToArray()
            };
            return Json(jsonData);
        }

        [HttpPost]
        public ActionResult CheckDate(string mdate)
        {
            DateTime entrydate = ParseToDate(mdate);//mdate format dd-mm-yyyy
            var plant = int.Parse(Session["PlantID"].ToString());
            var count = (from m in dbQIS.Main
                         where m.plant_id.Equals(plant) && m.entry_date.Day == entrydate.Day && m.entry_date.Month == entrydate.Month && m.entry_date.Year == entrydate.Year//m.entry_date.Equals(entrydate)
                         select m).Count();
            if (count == 0)
            {
                return Json("entrydate=01/01/0001");
            }
            else
            {
                return Json("entrydate=" + DateToString(entrydate,"/"));
            }
        }

        public DateTime ParseToDate(string inputDT)
        {
            char[] delimiters = new char[] { '-', '/', ' ' };
            var temp = inputDT.Split(delimiters);
            DateTime outputDT = DateTime.Parse(temp[2] + "-" + temp[1] + "-" + temp[0]);
            return outputDT;
        }

        public string DateToString(DateTime inputDT, string delimiter)
        {
            string outputDT = inputDT.Day + delimiter + inputDT.Month + delimiter + inputDT.Year;
            return outputDT;
        }

        //
        // GET: /QCReject/Edit/5
        //public ActionResult Edit(string dd, string mm, string yy)
        //{
        //    if (Session["QIS_Auth"] == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }
        //    if (Session["PlantID"] == null)
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }
        //    var plant_id = int.Parse(Session["PlantID"].ToString());
        //    DateTime dt = DateTime.Parse(yy + "-" + mm + "-" + dd);
        //    var inspection = dbQIS.InspectionLot.Where(q =>
        //        q.entry_date == dt && q.plant_id == plant_id);

        //    return View(inspection);
        //}

        //public ActionResult Edit(string rej_id)
        //{
        //    var qc_rej_id = int.Parse(rej_id);
        //    return View(dbQIS.QCReject.Where(q => q.qc_reject_id == qc_rej_id));
        //}

        public ActionResult UploadImages()
        {
            var path = @"~\Uploads\Images\" + DateTime.Now.Year + "\\";
            DirectoryInfo di = new DirectoryInfo(Server.MapPath(path));

            var file = Request.Files["Filedata"];
            var prefix = Request.Form["prefix"];
            string savePath = "";
            try
            {
                if (!di.Exists)
                {
                    di.Create();
                }
                savePath = Server.MapPath(path + prefix + file.FileName);
                file.SaveAs(savePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            return Content(savePath);
        }

        public ActionResult UploadFiles()
        {
            var path = @"~\Uploads\FileScan\" + DateTime.Now.Year + "\\";
            DirectoryInfo di = new DirectoryInfo(Server.MapPath(path));

            var file = Request.Files["Filedata"];
            var prefix = Request.Form["prefix"];
            
            string savePath = "";
            try
            {
                if (!di.Exists)
                {
                    di.Create();
                }
                savePath = Server.MapPath(path + prefix + file.FileName);
                file.SaveAs(savePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            return Content(savePath);
        }

        public string GetMgrByPlant(int plant_id)
        {
            var mgr_code = "";

            try
            {
                var selMgr = (from a in dbQIS.AuthUser
                              where a.plant_id == plant_id && a.utype_id == 3
                              select a).First();
                mgr_code = selMgr.user_code;
            }
            catch (Exception ex)
            {
                Response.Write("Error Get Mgr by Plant : " + ex);
            }
            return mgr_code;
        }

        public ActionResult SearchUsers(string searchTerm)
        {
            var user = dbCenter.Employee
                .Where(w => (w.FirstName.Contains(searchTerm) || w.FirstNameTh.Contains(searchTerm) || w.EmpId.Contains(searchTerm)))
                .OrderBy(o => o.FirstName)
                .Select(a => new { id = a.EmpId, text = a.FirstNameTh + " " + a.LastNameTh + " (" + a.EmpId + ")" })
                .Take(16).ToList();
            return Json(user, JsonRequestBehavior.AllowGet);
        }

        public void InsertMailCenter(string receiver, int qc_reject_id)
        {
            try
            {
                var query = (from q in dbQIS.QCReject
                             where q.qc_reject_id == qc_reject_id
                             select q).FirstOrDefault();
                MailCenterEntities dbMail = new MailCenterEntities();
                TT_MAIL_WIP ttMail = new TT_MAIL_WIP();
                ttMail.ProgramID = 3;
                ttMail.CreateDate = DateTime.Now;
                ttMail.Sender = "TNCAutoMail-QIS@nok.co.th";
                //ttMail.BCC = "monchit@nok.co.th";
                ttMail.Receiver = receiver;
                ttMail.Title = "QC Critical Reject : " + query.Master_Product.product_name + "/" + query.item_code + "/" + query.problem;
                string bodyhtml = "<b>Dear. All Concern</b>"
                + "<br /><b>*This message is automatic sending from QIS, please do not reply*</b>"
                + "<br /><br /><b>Critical Reject Detail :</b>";
                if (query.Car != null)
                {
                    bodyhtml += "<table><tr><td>Report No.</td><td>:</td><td>" + query.Car.report_no + "</td><tr>"
                + "<tr><td>Product Name</td><td>:</td><td>" + query.Master_Product.product_name + "</td><tr>"
                + "<tr><td>Item Code</td><td>:</td><td>" + query.item_code + "</td><tr>"
                + "<tr><td>Lot No.</td><td>:</td><td>" + query.lot_no + "</td><tr>"
                + "<tr><td>Problem Detail</td><td>:</td><td>" + query.problem + "</td><tr>"
                + "<tr><td>Reply Due Date</td><td>:</td><td>" + DateToString(query.Car.reply_due_date, "/") + "</td><tr>"
                + "<tr><td>More Detail</td><td>:</td><td><a href=\"http://webExternal/qis/qcreject/detail/" + query.qc_reject_id + "\">Show</a></td><tr>";
                }
                else
                {
                    bodyhtml += "<table><tr><td>Product Name</td><td>:</td><td>" + query.Master_Product.product_name + "</td><tr>"
                + "<tr><td>Item Code</td><td>:</td><td>" + query.item_code + "</td><tr>"
                + "<tr><td>Lot No.</td><td>:</td><td>" + query.lot_no + "</td><tr>"
                + "<tr><td>Problem Detail</td><td>:</td><td>" + query.problem + "</td><tr>"
                + "<tr><td>More Detail</td><td>:</td><td><a href=\"http://webExternal/qis/qcreject/detail/" + query.qc_reject_id + "\">Show</a></td><tr>";
                }

                bodyhtml += query.picture != null ? "<tr><td>Picture</td><td>:</td><td><a href=\"http://webExternal/qis/" + query.picture + "\">Show</a></td><tr>" : string.Empty;
                bodyhtml += "</table><br /><br /><b>Best Regard,<br />From Quality Information System</b>";

                ttMail.HTMLBody = bodyhtml;
                //ttMail.Flag = 1;//Comment this line when Real.

                var qinsert = dbMail.TT_MAIL_WIP.Add(ttMail);
                dbMail.SaveChanges();
            }
            catch (Exception ex)
            {
                Response.Write("Error InsertMailCenter : " + ex);
            }
        }

        public void InsertMail(string receiver, int qc_reject_id)
        {
            try
            {
                var query = (from q in dbQIS.QCReject
                             where q.qc_reject_id == qc_reject_id
                             select q).FirstOrDefault();
                MailCenterEntities dbMail = new MailCenterEntities();
                TT_MAIL_WIP ttMail = new TT_MAIL_WIP();
                ttMail.ProgramID = 3;
                ttMail.CreateDate = DateTime.Now;
                ttMail.Sender = "TNCAutoMail-QIS@nok.co.th";
                ttMail.Receiver = "monchit@nok.co.th";
                //ttMail.Receiver = receiver;

                ttMail.Title = "QC Reject Created.";
                string bodyhtml = "<b>Dear. All Concern</b>"
                + "<br /><b>*This message is automatic sending from QIS, please do not reply*</b>"
                + "<br /><br /><b>QC Reject Detail :</b>"
                + "<br />Product Name : " + query.Master_Product.product_name
                + "<br />Item Code : " + query.item_code
                + "<br />Lot No. : " + query.lot_no
                + "<br />Problem Detail : " + query.problem
                + "<br />More Detail : <a href=\"http://webExternal/qis/qcreject/detail/" + query.qc_reject_id + "\">Show</a>";


                ttMail.HTMLBody = bodyhtml;
                //ttMail.Flag = 1;//Comment this line when Real.

                var qinsert = dbMail.TT_MAIL_WIP.Add(ttMail);
                dbMail.SaveChanges();
            }
            catch (Exception ex)
            {
                Response.Write("Error InsertMail : " + ex);
            }
        }

        public void InsertTemp(string receiver, int qc_reject_id)
        {
            try{
                var emails = receiver.Split(',');
                foreach (string email in emails)
                {
                    if (email != "")
                    {
                        var tbTemp = new TempEmail();
                        tbTemp.qc_reject_id = qc_reject_id;
                        tbTemp.email = email;
                        tbTemp.create_date = DateTime.Now;
                        tbTemp.type = 1;// 1 = Approve Mail
                        dbQIS.TempEmail.Add(tbTemp);
                    }
                }
                dbQIS.SaveChanges();
            }
            catch (Exception ex)
            {
                Response.Write("Error InsertTemp : " + ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            dbQIS.Dispose();
            base.Dispose(disposing);
        }

        public string GetEmailQC(string empcode)
        {
            var get_group = (from a in dbTNC.tnc_user
                             where a.emp_code == empcode
                             select a).FirstOrDefault();
            if (get_group != null)
            {
                string email_list = "";
                if (get_group.emp_position == 6)
                {
                    var query = (from q in dbTNC.View_Organization
                                 where q.dept_id == get_group.emp_depart
                                 select q).FirstOrDefault();
                    if (query != null)
                    {
                        email_list += !string.IsNullOrEmpty(query.DeptMgr_email) ? "," + query.DeptMgr_email : string.Empty;
                        email_list += !string.IsNullOrEmpty(query.PlantMgr_email) ? "," + query.PlantMgr_email : string.Empty;
                    }
                }
                else
                {
                    var query = (from q in dbTNC.View_Organization
                                 where q.group_id == get_group.emp_group
                                 select q).FirstOrDefault();
                    
                    if (query != null)
                    {
                        email_list += !string.IsNullOrEmpty(query.GroupMgr_email) ? "," + query.GroupMgr_email : string.Empty;
                        email_list += !string.IsNullOrEmpty(query.DeptMgr_email) ? "," + query.DeptMgr_email : string.Empty;
                        email_list += !string.IsNullOrEmpty(query.PlantMgr_email) ? "," + query.PlantMgr_email : string.Empty;
                    }
                }
                return email_list;
            }
            else
            {
                return "";
            }
        }

        public string GetEmailProduction(int? group_id)
        {
            var query = (from q in dbTNC.View_Organization
                         where q.group_id == group_id
                         select q).FirstOrDefault();
            string email_list="";
            if (query != null)
            {
                email_list += !string.IsNullOrEmpty(query.GroupMgr_email) ? "," + query.GroupMgr_email : string.Empty;
                email_list += !string.IsNullOrEmpty(query.DeptMgr_email) ? "," + query.DeptMgr_email : string.Empty;
                email_list += !string.IsNullOrEmpty(query.PlantMgr_email) ? "," + query.PlantMgr_email : string.Empty;
                //if(email_list.Length > 0)
                //    email_list = email_list.Substring(1);
            }
            
            return email_list;
        }

        private string GetEmailProdSupDown(int? group_id)
        {
            var query = from a in dbTNC.tnc_user
                        where a.emp_group == group_id && (a.email != null && a.email != "")
                        && (a.emp_position == 3 || a.emp_position == 5)//Staff or Supervisor
                        select a.email;
            string email_list = "";
            if (query.Any())
            {
                foreach (var item in query)
                {
                    email_list += "," + item;
                }
                email_list = email_list.Substring(1);
            }
            return email_list;
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getGroupByProduct(int product)
        {
            var product_master = (from p in dbQIS.Master_Product
                                  where p.product_id == product && p.product_del_flag == false
                                  select new
                                  {
                                      group_id = p.group_id
                                  }).FirstOrDefault();
            return Json(product_master, JsonRequestBehavior.AllowGet);
        }
    }
}