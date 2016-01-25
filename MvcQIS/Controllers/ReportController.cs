using MvcQIS.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace MvcQIS.Controllers
{
    public class ReportController : Controller
    {
        QISEntities dbQIS = new QISEntities();
        TNC_CenterEntities dbCenter = new TNC_CenterEntities();
        TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();
        //
        // GET: /Report/

        public ActionResult QCReject_Detail()
        {
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Title = "QC Reject Detail";
            var plant_master = (from pl in dbQIS.Master_Plant
                                select pl);
            ViewBag.SelectPlant = plant_master;

            var product_master = (from pr in dbQIS.Master_Product
                                  where pr.product_del_flag == false
                                  orderby pr.product_name
                                  select pr);
            ViewBag.SelectProduct = product_master;
            return View();
        }

        public ActionResult CAR_PAR_FollowUp()
        {
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Title = "CAR/PAR FollowUp";
            var plant_master = (from pl in dbQIS.Master_Plant
                                select pl);
            ViewBag.SelectPlant = plant_master;

            var product_master = (from pr in dbQIS.Master_Product
                                  where pr.product_del_flag == false
                                  orderby pr.product_name
                                  select pr);
            ViewBag.SelectProduct = product_master;
            return View();
        }

        public ActionResult Graph()
        {
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Title = "Graph";
            var plant_master = (from pl in dbQIS.Master_Plant
                                where pl.plant_id > 0
                                select pl);
            ViewBag.SelectPlant = plant_master;
            ViewBag.MyPlant = Session["PlantID"] != null ? Session["PlantID"].ToString() : "0";
            return View();
        }

        public ActionResult Man_Statistical()
        {
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Title = "Man Statistical";
            var plant_master = from pl in dbQIS.Master_Plant
                               select pl;
            ViewBag.SelectPlant = plant_master;
            ViewBag.MyPlant = Session["PlantID"] != null ? Session["PlantID"].ToString() : "0";
            var product_master = (from pr in dbQIS.Master_Product
                                  where pr.product_del_flag == false
                                  orderby pr.product_name
                                  select pr);
            ViewBag.SelectProduct = product_master;
            var opertype = from op in dbQIS.Master_OperateUType
                           where op.outype_id > 1
                           orderby op.outype_id descending
                           select op;
            ViewBag.Oper = opertype;
            return View();
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getProductByPlant(int plantid)
        {
            var product_master = (from p in dbQIS.Master_Product
                                  where p.product_del_flag == false
                                  select new { 
                                      plant_id = p.plant_id,
                                      product_id = p.product_id,
                                      product_name = p.product_name
                                  });
            if (plantid != 0)
            {
                product_master = product_master.Where(p => p.plant_id == plantid);
            }
            //product_master = product_master.OrderBy(p => p.product_name);

            //object output = new
            //{
            //    product = product_master.ToList()
            //};

            return Json(product_master.ToList(),JsonRequestBehavior.AllowGet);
        }

        public ActionResult GridCriticalRejDetail(string sidx, string sord, int page, int rows, string search, int plantid, int productid, string startdate, string enddate)
        {
            CultureInfo enUS = new CultureInfo("en-US");
            
            DateTime startValue, endValue;
            var dbSel = from d in dbQIS.QCReject
                        where d.status_id == 4//Approved
                        select d;
            if (plantid != 0)
                dbSel = dbSel.Where(q => q.plant_id == plantid);

            if (productid != 0)
                dbSel = dbSel.Where(q => q.product_id == productid);

            if (DateTime.TryParseExact(startdate, "dd-MM-yyyy", enUS, DateTimeStyles.None, out startValue))
                dbSel = dbSel.Where(q => q.entry_date >= startValue);

            if (DateTime.TryParseExact(enddate, "dd-MM-yyyy", enUS, DateTimeStyles.None, out endValue))
                dbSel = dbSel.Where(q => q.entry_date <= endValue);

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
                        m.lot_no,
                        m.problem,
                        m.defective_lv,
                        m.picture != null ? "<a class='fancybox' href='" + Url.Content("~/" + m.picture) + "'><img src='" + Url.Content("~/" + m.picture) + "' class='gridImage' /></a>" : "<img src='" + Url.Content("~/" + "Images/noimage.jpg") + "' class='gridImage' />",
                        m.issue_car == 1 ? "<a href='#' class='btCar' data-rejId=" + m.qc_reject_id + "><img src='" + Url.Content("~/" + "Images/Document-icon.png") + "' /></a>" : ""
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        public ActionResult GridOverDue(string sidx, string sord, int page, int rows, string search, int plantid)
        {
            DateTime newDate = DateTime.Now;

            var dbSel = from c in dbQIS.Car.Where(w => newDate > w.reply_due_date && w.QCReject.status_id == 4
                        && w.reply_date == null).ToList()
                        join t in dbTNC.tnc_group_master.ToList() on c.responsible equals t.id into j
                        from t in j.DefaultIfEmpty()
                        //where newDate > c.reply_due_date //&& c.QCReject.status_id == 4 && c.QCReject.defective_lv != "Critical"
                        //&& c.reply_date == null
                        select new { groupname = t.group_name == null ? "N/A" : t.group_name, qc = c };
            if (plantid != 0)
            {
                dbSel = dbSel.Where(c => c.qc.QCReject.plant_id == plantid);
            }
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
                        q.qc.reply_due_date,
                        //q.reply_date.Value.Subtract(q.reply_due_date).Days,
                        newDate.Subtract(q.qc.reply_due_date).Days
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        public JsonResult GetPieChartData(int plantid, int productid, string startdate, string enddate)
        {
            CultureInfo enUS = new CultureInfo("en-US");
            DateTime startValue, endValue;
            var ChartData = dbQIS.Car.Where(w => w.QCReject.status_id != 9).AsQueryable();
            if (plantid != 0)
                ChartData = ChartData.Where(c => c.plant_id == plantid);

            if (productid != 0)
                ChartData = ChartData.Where(c => c.responsible == productid);

            if (DateTime.TryParseExact(startdate, "dd-MM-yyyy", enUS, DateTimeStyles.None, out startValue))
                ChartData = ChartData.Where(c => c.entry_date >= startValue);

            if (DateTime.TryParseExact(enddate, "dd-MM-yyyy", enUS, DateTimeStyles.None, out endValue))
                ChartData = ChartData.Where(c => c.entry_date <= endValue);

            var json = from c in ChartData
                       group c by c.root_cause into g
                       select new
                       {
                           rootcause = g.Key,
                           rootcount = g.Count()
                       };
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public JsonResult GetCurrentDateData(int mm, int yy, int plantid)
        {
            //var prd = from p in dbQIS.Master_Product
            //          where p.plant_id == plantid
            //          select p;
            
            //DateTime startOfMonth = new DateTime(yy, mm, 1);
            //DateTime endOfMonth = startOfMonth.AddDays(-1);

            //-------------------------------------//

            //DateTime entrydate = DateTime.Parse(mdate);

            //var mm = entrydate.Month;
            //var yy = entrydate.Year;
            //var plantid = int.Parse(Session["PlantID"].ToString());
            var entrydate = new DateTime(yy, mm, 1);

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

                            rate += "<td>" + string.Format("{0:#,0.00}", per) + "</td>";
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

            return Json(table, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public JsonResult GetCurrentDateGraphData(int mm, int yy, int plantid)
        {
            var prd = from p in dbQIS.Master_Product
                      where p.plant_id == plantid && p.product_del_flag == false
                      select p;

            DateTime startOfMonth = new DateTime(yy, mm+1, 1);
            DateTime endOfMonth = startOfMonth.AddDays(-1);

            string table = string.Empty, total = string.Empty, ng = string.Empty, rate = string.Empty, tbTotal = string.Empty, tbNG = string.Empty, tbRate = string.Empty;

            int[] totalSum = new int[endOfMonth.Day + 1];
            int[] totalNG = new int[endOfMonth.Day + 1];

            table += "<table id='datatable' class='bordered'>";
            //-------------Loop date----------------//
            table += "<thead><tr><th>Line</th>";
            for (int i = 1; i <= endOfMonth.Day; i++)
            {
                table += "<th>" + i + "</th>";
            }
            table += "</tr></thead><tbody>";
            //-------------Loop Products----------------//
            foreach (var item in prd)
            {
                total = "<th>" + item.product_name + "</th>";
                rate = "";
                //total += "<th>Total</th>";
                //ng = "<th>NG</th>";
                //rate = "<th>Rate(%)</th>";

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
                        //total += "<td>" + inspection.qty + "</td>";
                        totalSum[i] += inspection.qty;

                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date.Month == mm && q.entry_date.Year == yy
                                        && q.entry_date.Day == i && q.issue_car == 1
                                        orderby q.entry_date
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            //ng += "<td class='ngColor'>" + qcreject + "</td>";
                            totalNG[i] += qcreject;
                            double? per = 0.0;
                            if (inspection.qty != 0)
                                per = (qcreject * 100.0) / inspection.qty;

                            rate += "<td>" + string.Format("{0:#,0.00}", per) + "</td>";
                        }
                        else
                        {
                            //ng += "<td>0</td>";
                            rate += "<td>0</td>";
                        }
                    }
                    else
                    {
                        //total += "<td></td>";
                        //ng += "<td></td>";
                        rate += "<td></td>";
                    }
                }
                //table += "<tr>" + total + "</tr><tr>" + ng + "</tr><tr>" + rate + "</tr>";
                table += "<tr>" + total + rate + "</tr>";
            }

            //tbTotal = "<th>Total</th>";
            //tbNG = "<th>NG</th>";
            //tbRate = "<th>Rate(%)</th>";
            for (int i = 1; i <= endOfMonth.Day; i++)
            {
                //tbTotal += "<td>" + totalSum[i] + "</td>";
                //tbNG += "<td>" + totalNG[i] + "</td>";
                double sumRate = 0.0;
                if (totalSum[i] != 0 && totalNG[i] != 0)
                    sumRate = (totalNG[i] * 100.0) / totalSum[i];

                tbRate += "<td>" + string.Format("{0:#,0.##}", sumRate) + "</td>";
            }
            //table += "<tr>" + tbTotal + "</tr><tr>" + tbNG + "</tr><tr>" + tbRate + "</tr>";
            table += "<tr><th>Total</th>" + tbRate + "</tr>";
            table += "</tbody></table>";
            //table += "<p><input type='button' id='btnExport' value=' Export Table data into Excel ' /></p>";

            return Json(table, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getData_CurrentDate(int mm, int yy, int plantid)
        {
            var prd = from p in dbQIS.Master_Product
                      where p.plant_id == plantid && p.product_del_flag == false
                      select p;

            DateTime startOfMonth = mm != 12 ? new DateTime(yy, mm + 1, 1) : new DateTime(yy, 1, 1);
            DateTime endOfMonth = startOfMonth.AddDays(-1);

            Dictionary<string, double[]> retVal = new Dictionary<string, double[]>();

            int[] totalSum = new int[endOfMonth.Day];
            int[] totalNG = new int[endOfMonth.Day];

            foreach (var item in prd)
            {
                double[] rate = new double[endOfMonth.Day];
                for (int i = 0; i < endOfMonth.Day; i++)
                {
                    var inspection = (from x in dbQIS.InspectionLot
                                      where x.plant_id == plantid && x.product_id == item.product_id
                                      && x.entry_date.Month == mm && x.entry_date.Year == yy
                                      && x.entry_date.Day == (i + 1)
                                      orderby x.entry_date
                                      select x).FirstOrDefault();
                    if (inspection != null)
                    {
                        totalSum[i] += inspection.qty;
                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date.Month == mm && q.entry_date.Year == yy
                                        && q.entry_date.Day == (i + 1) && q.status_id == 4
                                        orderby q.entry_date
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            totalNG[i] += qcreject;
                            rate[i] = inspection.qty != 0 ? (qcreject * 100.0) / inspection.qty : 0;
                        }
                        else
                        {
                            rate[i] = 0;
                        }
                        //rate[i] = inspection.qty;
                    }
                    else
                    {
                        rate[i] = 0;
                    }
                }
                retVal.Add(item.product_name, rate);
            }
            //for total
            double[] Totalrate = new double[endOfMonth.Day];
            for (int i = 0; i < endOfMonth.Day; i++)
            {
                if (totalSum[i] != 0 && totalNG[i] != 0)
                    Totalrate[i] = (totalNG[i] * 100.0) / totalSum[i];
                else
                    Totalrate[i] = 0;
            }
            retVal.Add("Total", Totalrate);

            return Json(retVal.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getData_Monthly(int yy, int plantid)
        {
            var prd = from p in dbQIS.Master_Product
                      where p.plant_id == plantid && p.product_del_flag == false
                      select p;

            int monthOfYear = 12;
            Dictionary<string, double[]> retVal = new Dictionary<string, double[]>();

            int[] totalSum = new int[monthOfYear];
            int[] totalNG = new int[monthOfYear];
            
            foreach (var item in prd)
            {
                double[] rate = new double[monthOfYear];
                for (int i = 0; i < monthOfYear; i++)
                {
                    var inspection = (from x in dbQIS.InspectionLot
                                      where x.plant_id == plantid && x.product_id == item.product_id
                                      && x.entry_date.Year == yy && x.entry_date.Month == (i + 1)
                                      group x by x.entry_date.Month into g
                                      select new
                                      {
                                          Month = g.Key,
                                          Qty = g.Sum(x => x.qty)
                                      }).FirstOrDefault();

                    var tmp = 0;
                    if (inspection != null)
                    {
                        tmp = (int)inspection.Qty;
                        totalSum[i] += tmp;
                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date.Year == yy
                                        && q.entry_date.Month == (i + 1) && q.status_id == 4
                                        orderby q.entry_date
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            totalNG[i] += qcreject;
                            rate[i] = inspection.Qty != 0 ? (qcreject * 100.0) / tmp : 0;
                        }
                        else
                        {
                            rate[i] = 0;
                        }
                    }
                    else
                    {
                        rate[i] = 0;
                    }
                }
                retVal.Add(item.product_name, rate);
            }
            //for total
            double[] Totalrate = new double[monthOfYear];
            for (int i = 0; i < monthOfYear; i++)
            {
                if (totalSum[i] != 0 && totalNG[i] != 0)
                    Totalrate[i] = (totalNG[i] * 100.0) / (double)totalSum[i];
                else
                    Totalrate[i] = 0;
                //Totalrate[i] = totalNG[i];
            }
            retVal.Add("Total", Totalrate);

            return Json(retVal.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getData_Yearly(int plantid)
        {
            var prd = from p in dbQIS.Master_Product
                      where p.plant_id == plantid && p.product_del_flag == false
                      select p;

            var allYear = (from y in dbQIS.Main
                          select y.entry_date.Year).Distinct();

            int countYear = allYear.Count();

            Dictionary<string, double[]> retVal = new Dictionary<string, double[]>();

            int[] totalSum = new int[countYear];
            int[] totalNG = new int[countYear];

            foreach (var item in prd)
            {
                double[] rate = new double[countYear];
                int i = 0;
                foreach (var year in allYear)
                {
                    int tmp = 0;
                    var inspection = (from x in dbQIS.InspectionLot
                                      where x.plant_id == plantid && x.product_id == item.product_id
                                      && x.entry_date.Year == year
                                      group x by x.entry_date.Year into g
                                      select new
                                      {
                                          Year = g.Key,
                                          Qty = g.Sum(x => x.qty)
                                      }).FirstOrDefault();

                    if (inspection != null)
                    {
                        tmp = (int)inspection.Qty;
                        totalSum[i] += tmp;
                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date.Year == year && q.status_id == 4
                                        orderby q.entry_date
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            totalNG[i] += qcreject;
                            rate[i] = inspection.Qty != 0 ? (qcreject * 100.0) / tmp : 0;
                        }
                        else
                        {
                            rate[i] = 0;
                        }
                    }
                    i++;
                }
                retVal.Add(item.product_name, rate);
            }

            //for total
            double[] Totalrate = new double[countYear];
            for (int i = 0; i < countYear; i++)
            {
                if (totalSum[i] != 0 && totalNG[i] != 0)
                    Totalrate[i] = (totalNG[i] * 100.0) / (double)totalSum[i];
                else
                    Totalrate[i] = 0;
            }
            retVal.Add("Total", Totalrate);

            return Json(retVal.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getData_HalfYear(int plantid)
        {
            var prd = from p in dbQIS.Master_Product
                      where p.plant_id == plantid && p.product_del_flag == false
                      select p;

            int countYear = DateTime.Now.Month > 6 ? 1 : 2;
            int startYear = DateTime.Now.Year - countYear + 1;
            
            Dictionary<string, double[]> retVal = new Dictionary<string, double[]>();

            int[] totalSum = new int[2];
            int[] totalNG = new int[2];

            foreach (var item in prd)
            {
                double[] rate = new double[2];
                if (countYear == 1)//one year - this year
                {
                    int tmp1 = 0;
                    int tmp2 = 0;
                    var inspection1 = (from x in dbQIS.InspectionLot
                                      where x.plant_id == plantid && x.product_id == item.product_id
                                      && x.entry_date.Year == startYear && x.entry_date.Month < 7
                                      group x by x.product_id into g
                                      select new
                                      {
                                          Product = g.Key,
                                          Qty = g.Sum(x => x.qty)
                                      }).FirstOrDefault();

                    var inspection2 = (from x in dbQIS.InspectionLot
                                       where x.plant_id == plantid && x.product_id == item.product_id
                                       && x.entry_date.Year == startYear && x.entry_date.Month > 6
                                       group x by x.product_id into g
                                       select new
                                       {
                                           Product = g.Key,
                                           Qty = g.Sum(x => x.qty)
                                       }).FirstOrDefault();

                    if (inspection1 != null)
                    {
                        tmp1 = (int)inspection1.Qty;
                        totalSum[0] += tmp1;
                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date.Year == (startYear) && q.status_id == 4 && q.entry_date.Month < 7
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            totalNG[0] += qcreject;
                            rate[0] = inspection1.Qty != 0 ? (qcreject * 100.0) / tmp1 : 0;
                        }
                        else
                        {
                            rate[0] = 0;
                        }
                    }
                    if (inspection2 != null)
                    {
                        tmp2 = (int)inspection2.Qty;
                        totalSum[1] += tmp2;
                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date.Year == (startYear) && q.status_id == 4 && q.entry_date.Month > 6
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            totalNG[1] += qcreject;
                            rate[1] = inspection2.Qty != 0 ? (qcreject * 100.0) / tmp2 : 0;
                        }
                        else
                        {
                            rate[1] = 0;
                        }
                    }
                }
                else//two year - this year & last year
                {
                    int tmp1 = 0;
                    int tmp2 = 0;
                    var inspection1 = (from x in dbQIS.InspectionLot
                                       where x.plant_id == plantid && x.product_id == item.product_id
                                       && x.entry_date.Year == startYear && x.entry_date.Month > 6
                                       group x by x.product_id into g
                                       select new
                                       {
                                           Product = g.Key,
                                           Qty = g.Sum(x => x.qty)
                                       }).FirstOrDefault();

                    var inspection2 = (from x in dbQIS.InspectionLot
                                       where x.plant_id == plantid && x.product_id == item.product_id
                                       && x.entry_date.Year == startYear + 1 && x.entry_date.Month < 7
                                       group x by x.product_id into g
                                       select new
                                       {
                                           Product = g.Key,
                                           Qty = g.Sum(x => x.qty)
                                       }).FirstOrDefault();

                    if (inspection1 != null)
                    {
                        tmp1 = (int)inspection1.Qty;
                        totalSum[0] += tmp1;
                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date.Year == (startYear) && q.status_id == 4 && q.entry_date.Month > 6
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            totalNG[0] += qcreject;
                            rate[0] = inspection1.Qty != 0 ? (qcreject * 100.0) / tmp1 : 0;
                        }
                        else
                        {
                            rate[0] = 0;
                        }
                    }
                    if (inspection2 != null)
                    {
                        tmp2 = (int)inspection2.Qty;
                        totalSum[1] += tmp2;
                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date.Year == (startYear + 1) && q.status_id == 4 && q.entry_date.Month < 7
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            totalNG[1] += qcreject;
                            rate[1] = inspection2.Qty != 0 ? (qcreject * 100.0) / tmp2 : 0;
                        }
                        else
                        {
                            rate[1] = 0;
                        }
                    }
                }

                retVal.Add(item.product_name, rate);
            }

            //for total
            double[] Totalrate = new double[2];
            for (int i = 0; i < 2; i++)
            {
                if (totalSum[i] != 0 && totalNG[i] != 0)
                    Totalrate[i] = (totalNG[i] * 100.0) / (double)totalSum[i];
                else
                    Totalrate[i] = 0;
                //Totalrate[i] = totalSum[i];
            }
            retVal.Add("Total", Totalrate);

            return Json(retVal.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getData_LastYear(int plantid)
        {
            var prd = from p in dbQIS.Master_Product
                      where p.plant_id == plantid && p.product_del_flag == false
                      select p;

            int startYear = DateTime.Now.Year - 1;

            int countYear = 2;

            Dictionary<string, double[]> retVal = new Dictionary<string, double[]>();

            int[] totalSum = new int[countYear];
            int[] totalNG = new int[countYear];

            foreach (var item in prd)
            {
                double[] rate = new double[countYear];
                for (int i = 0; i < countYear; i++)
                {
                    int tmp = 0;

                    var inspection = (from x in dbQIS.InspectionLot
                                      where x.plant_id == plantid && x.product_id == item.product_id
                                      && x.entry_date.Year == (startYear + i)
                                      group x by x.entry_date.Year into g
                                      select new
                                      {
                                          Year = g.Key,
                                          Qty = g.Sum(x => x.qty)
                                      }).FirstOrDefault();

                    if (inspection != null)
                    {
                        tmp = (int)inspection.Qty;
                        totalSum[i] += tmp;
                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date.Year == (startYear + i) && q.status_id == 4
                                        orderby q.entry_date
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            totalNG[i] += qcreject;
                            rate[i] = inspection.Qty != 0 ? (qcreject * 100.0) / tmp : 0;
                        }
                        else
                        {
                            rate[i] = 0;
                        }
                    }
                }
                retVal.Add(item.product_name, rate);
            }

            //for total
            double[] Totalrate = new double[countYear];
            for (int i = 0; i < countYear; i++)
            {
                if (totalSum[i] != 0 && totalNG[i] != 0)
                    Totalrate[i] = (totalNG[i] * 100.0) / (double)totalSum[i];
                else
                    Totalrate[i] = 0;
            }
            retVal.Add("Total", Totalrate);

            return Json(retVal.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getData_HLMonthly(int plantid)
        {
            var prd = from p in dbQIS.Master_Product
                      where p.plant_id == plantid && p.product_del_flag == false
                      select p;

            int countYear = DateTime.Now.Month > 6 ? 1 : 2;
            int startYear = DateTime.Now.Year - countYear + 1;
            int monthOfYear = 12;
            Dictionary<string, double[]> retVal = new Dictionary<string, double[]>();

            int[] totalSum = new int[monthOfYear];
            int[] totalNG = new int[monthOfYear];

            foreach (var item in prd)
            {
                double[] rate = new double[monthOfYear];
                if (countYear == 1)//one year - this year
                {
                    for (int i = 0; i < monthOfYear; i++)
                    {
                        var inspection = (from x in dbQIS.InspectionLot
                                          where x.plant_id == plantid && x.product_id == item.product_id
                                          && x.entry_date.Month == (i + 1) && x.entry_date.Year == startYear
                                          group x by x.entry_date.Month into g
                                          select new
                                          {
                                              Month = g.Key,
                                              Qty = g.Sum(x => x.qty)
                                          }).FirstOrDefault();

                        var tmp = 0;
                        if (inspection != null)
                        {
                            tmp = (int)inspection.Qty;
                            totalSum[i] += tmp;
                            var qcreject = (from q in dbQIS.QCReject
                                            where q.plant_id == plantid && q.product_id == item.product_id
                                            && q.entry_date.Month == (i + 1) && q.entry_date.Year == startYear && q.status_id == 4
                                            orderby q.entry_date
                                            select q).Count();
                            if (qcreject > 0)
                            {
                                totalNG[i] += qcreject;
                                rate[i] = inspection.Qty != 0 ? (qcreject * 100.0) / tmp : 0;
                            }
                            else { rate[i] = 0; }
                        }
                        else { rate[i] = 0; }
                    }
                }
                else if (countYear == 2)//two year
                {
                    for (int i = 6; i < 12; i++)
                    {
                        var inspection1 = (from x in dbQIS.InspectionLot
                                           where x.plant_id == plantid && x.product_id == item.product_id
                                           && x.entry_date.Year == startYear && x.entry_date.Month == (i + 1)
                                           group x by x.entry_date.Month into g
                                           select new
                                           {
                                               Month = g.Key,
                                               Qty = g.Sum(x => x.qty)
                                           }).FirstOrDefault();
                        int tmp1 = 0;
                        if (inspection1 != null)
                        {
                            tmp1 = (int)inspection1.Qty;
                            totalSum[i-6] += tmp1;
                            var qcreject = (from q in dbQIS.QCReject
                                            where q.plant_id == plantid && q.product_id == item.product_id
                                            && q.entry_date.Year == startYear
                                            && q.entry_date.Month == (i + 1) && q.status_id == 4
                                            orderby q.entry_date
                                            select q).Count();
                            if (qcreject > 0)
                            {
                                totalNG[i-6] += qcreject;
                                rate[i-6] = inspection1.Qty != 0 ? (qcreject * 100.0) / tmp1 : 0;
                            }
                            else
                            {
                                rate[i-6] = 0;
                            }
                        }
                        else
                        {
                            rate[i-6] = 0;
                        }
                        
                    }

                    for (int j = 0; j < 6; j++)
                    {
                        var inspection2 = (from x in dbQIS.InspectionLot
                                           where x.plant_id == plantid && x.product_id == item.product_id
                                           && x.entry_date.Year == (startYear + 1) && x.entry_date.Month == (j + 1)
                                           group x by x.entry_date.Month into g
                                           select new
                                           {
                                               Month = g.Key,
                                               Qty = g.Sum(x => x.qty)
                                           }).FirstOrDefault();
                        int tmp2 = 0;
                        if (inspection2 != null)
                        {
                            tmp2 = (int)inspection2.Qty;
                            totalSum[j+6] += tmp2;
                            var qcreject = (from q in dbQIS.QCReject
                                            where q.plant_id == plantid && q.product_id == item.product_id
                                            && q.entry_date.Year == (startYear + 1)
                                            && q.entry_date.Month == (j + 1) && q.status_id == 4
                                            orderby q.entry_date
                                            select q).Count();
                            if (qcreject > 0)
                            {
                                totalNG[j+6] += qcreject;
                                rate[j+6] = inspection2.Qty != 0 ? (qcreject * 100.0) / tmp2 : 0;
                            }
                            else
                            {
                                rate[j+6] = 0;
                            }
                        }
                        else
                        {
                            rate[j+6] = 0;
                        }
                    }
                }
                
                retVal.Add(item.product_name, rate);
            }
            //for total
            double[] Totalrate = new double[monthOfYear];
            for (int i = 0; i < monthOfYear; i++)
            {
                if (totalSum[i] != 0 && totalNG[i] != 0)
                    Totalrate[i] = (totalNG[i] * 100.0) / (double)totalSum[i];
                else
                    Totalrate[i] = 0;
            }
            retVal.Add("Total", Totalrate);

            return Json(retVal.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getData_ManMonth(int mm, int yy, int plantid, int productid, int utype)
        {
            int[] man_data = new int[10];
            string[] username = new string[10];
            var query = dbQIS.OperateUser.Where(w => w.entry_date.Month == mm && w.entry_date.Year == yy && w.outype_id == utype);
            if (plantid != 0)
                query = query.Where(q => q.plant_id == plantid);

            if (productid != 0)
                query = query.Where(q => q.QCReject.product_id == productid);

            var oper = query.GroupBy(g => g.emp_id).OrderByDescending(o => o.Count())
                    .Select(g => new { user = g.Key, ucount = g.Count() }).Take(10);
            //var oper = (from o in dbQIS.OperateUser
            //           where o.entry_date.Month == mm && o.entry_date.Year == yy && o.outype_id == utype
            //           group o by o.emp_id into g
            //           orderby g.Count() descending
            //           select new { user = g.Key, ucount = g.Count() }).Take(10);
            int i = 0;
            foreach (var item in oper)
            {
                var user = (from u in dbCenter.Employee
                           where u.EmpId == item.user
                           select u).FirstOrDefault();
                man_data[i] = item.ucount;
                username[i] = user.FirstNameTh + "<br />" + user.LastNameTh;
                i++;
            }
            object data = new { cat = username, ucount = man_data };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getData_ManYear(int yy, int plantid, int productid, int utype)
        {
            int[] man_data = new int[10];
            string[] username = new string[10];
            var query = dbQIS.OperateUser.Where(w => w.entry_date.Year == yy && w.outype_id == utype);
            if (plantid != 0)
                query = query.Where(q => q.plant_id == plantid);

            if (productid != 0)
                query = query.Where(q => q.QCReject.product_id == productid);

            var oper = query.GroupBy(g => g.emp_id).OrderByDescending(o => o.Count())
                    .Select(g => new { user = g.Key, ucount = g.Count() }).Take(10);
            //var oper = (from o in dbQIS.OperateUser
            //            where o.entry_date.Year == yy && o.outype_id == utype
            //            group o by o.emp_id into g
            //            orderby g.Count() descending
            //            select new { user = g.Key, ucount = g.Count() }).Take(10);
            int i = 0;
            foreach (var item in oper)
            {
                var user = (from u in dbCenter.Employee
                            where u.EmpId == item.user
                            select u).FirstOrDefault();
                man_data[i] = item.ucount;
                username[i] = user.FirstNameTh + "<br />" + user.LastNameTh;
                i++;
            }
            object data = new { cat = username, ucount = man_data };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public ActionResult getData_DayToDay(int plantid, DateTime dtFrom, DateTime dtTo)
        {
            var prd = from p in dbQIS.Master_Product
                      where p.plant_id == plantid && p.product_del_flag == false
                      select p;

            //DateTime startOfMonth = mm != 12 ? new DateTime(yy, mm + 1, 1) : new DateTime(yy, 1, 1);
            //DateTime endOfMonth = startOfMonth.AddDays(-1);
            int dtCount = (dtTo.Date - dtFrom.Date).Days;

            Dictionary<string, double[]> retVal = new Dictionary<string, double[]>();

            int[] totalSum = new int[dtCount+1];
            int[] totalNG = new int[dtCount+1];

            foreach (var item in prd)
            {
                double[] rate = new double[dtCount+1];
                for (int i = 0; i <= dtCount; i++)
                {
                    DateTime dt = dtFrom.AddDays(Convert.ToDouble(i));
                    var inspection = (from x in dbQIS.InspectionLot
                                      where x.plant_id == plantid && x.product_id == item.product_id
                                      && x.entry_date == dt
                                      orderby x.entry_date
                                      select x).FirstOrDefault();
                    if (inspection != null)
                    {
                        totalSum[i] += inspection.qty;
                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        //&& q.entry_date.Month == mm && q.entry_date.Year == yy
                                        //&& q.entry_date.Day == (i + 1) 
                                        && q.entry_date == dt && q.status_id == 4
                                        orderby q.entry_date
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            totalNG[i] += qcreject;
                            rate[i] = inspection.qty != 0 ? (qcreject * 100.0) / inspection.qty : 0;
                        }
                        else
                        {
                            rate[i] = 0;
                        }
                        //rate[i] = inspection.qty;
                    }
                    else
                    {
                        rate[i] = 0;
                    }
                }
                retVal.Add(item.product_name, rate);
            }
            //for total
            double[] Totalrate = new double[dtCount+1];
            for (int i = 0; i <= dtCount; i++)
            {
                if (totalSum[i] != 0 && totalNG[i] != 0)
                    Totalrate[i] = (totalNG[i] * 100.0) / totalSum[i];
                else
                    Totalrate[i] = 0;
            }
            retVal.Add("Total", Totalrate);

            return Json(retVal.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 0, VaryByParam = "*", NoStore = true)]
        public JsonResult GetDayToDayData(int plantid, DateTime dtFrom, DateTime dtTo)
        {
            var prd = from p in dbQIS.Master_Product
                      where p.plant_id == plantid && p.product_del_flag == false
                      select p;

            int dtCount = (dtTo.Date - dtFrom.Date).Days;
            //int startDate = dtFrom.Day;
            //DateTime newEntryDate = entrydate.AddMonths(1);
            //DateTime startOfMonth = new DateTime(newEntryDate.Year, newEntryDate.Month, 1);
            //var endOfMonth = startOfMonth.AddDays(-1);

            string table = string.Empty, total = string.Empty, ng = string.Empty, rate = string.Empty, tbTotal = string.Empty, tbNG = string.Empty, tbRate = string.Empty;

            int[] totalSum = new int[dtCount + 1];
            int[] totalNG = new int[dtCount + 1];

            table += "<table id='tbDayToDay' class='bordered'>";
            //-------------Loop date----------------//
            table += "<tr><th>Line</th><th>Lot</th>";
            for (int i = 0; i <= dtCount; i++)
            {
                table += "<th>" + dtFrom.AddDays(i).Day + "</th>";
            }
            table += "</tr>";
            //-------------Loop Products----------------//
            foreach (var item in prd)
            {
                total = "<th rowspan='3'>" + item.product_name + "</th>";

                total += "<th>Total</th>";
                ng = "<th>NG</th>";
                rate = "<th>Rate(%)</th>";
                
                for (int i = 0; i <= dtCount; i++)
                {
                    DateTime dt = dtFrom.AddDays(Convert.ToDouble(i));
                    var inspection = (from x in dbQIS.InspectionLot
                                      where x.plant_id == plantid && x.product_id == item.product_id
                                      && x.entry_date == dt
                                      orderby x.entry_date
                                      select x).FirstOrDefault();

                    if (inspection != null)
                    {
                        total += "<td>" + inspection.qty + "</td>";
                        totalSum[i] += inspection.qty;

                        var qcreject = (from q in dbQIS.QCReject
                                        where q.plant_id == plantid && q.product_id == item.product_id
                                        && q.entry_date == dt && q.status_id == 4
                                        orderby q.entry_date
                                        select q).Count();
                        if (qcreject > 0)
                        {
                            ng += "<td class='ngColor tooltipUI' data-date='" + dt.Day + "' data-month='" + dt.Month + "' data-year='" + dt.Year + "' data-product='" + item.product_id + "'>" + qcreject + "</td>";
                            totalNG[i] += qcreject;
                            double? per = 0.0;
                            if (inspection.qty != 0)
                                per = (qcreject * 100.0) / inspection.qty;

                            rate += "<td>" + string.Format("{0:#,0.00}", per) + "</td>";
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
            for (int i = 0; i <= dtCount; i++)
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

            return Json(table, JsonRequestBehavior.AllowGet);
        }
    }
}
