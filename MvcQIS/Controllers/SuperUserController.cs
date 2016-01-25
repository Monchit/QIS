using MvcQIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using MvcJqGrid;

namespace MvcQIS.Controllers
{
    public class SuperUserController : Controller
    {
        //
        // GET: /SuperUser/
        QISEntities dbQIS = new QISEntities();
        TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();

        public ActionResult Index()
        {
            ViewBag.Title = "SuperUser Function";
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (Session["UserType"].ToString() != "1")// 1 = SuperUser
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }

        //-----------------------------------User Master--------------------------------------//
        public ActionResult GridUserData(string sidx, string sord, int page, int rows, string search)
        {
            var plant = int.Parse(Session["PlantID"].ToString());
            var db = dbQIS.AuthUser.Where(u => u.plant_id == plant && u.utype_id > 1);
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = db.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);
            
            // This is possible because I'm using the LINQ Dynamic Query Library
            //var users = db.OrderBy(sidx + " " + sord)
            //        .Skip(pageIndex * pageSize)
            //        .Take(pageSize).AsEnumerable();
            var users = (from u in db.ToList()
                         join t in dbTNC.tnc_user.ToList()
                         on u.user_code equals t.emp_code
                         select new
                         {
                             user_code = u.user_code,
                             empfullname = t.emp_fname + " " + t.emp_lname,
                             utype_id = u.Master_UType.utype_name,
                             plant_id = u.Master_Plant.plant_name
                         }).AsQueryable()
                         .OrderBy(sidx + " " + sord)
                         .Skip(pageIndex * pageSize)
                         .Take(pageSize).AsEnumerable();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = users.Select(u => new
                {
                    i = u.user_code,
                    cell = new object[] {
                        u.user_code,
                        u.empfullname,
                        u.utype_id,
                        u.plant_id
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        public ActionResult AddUser(string user_code, string name, byte utype_id)
        {
            try
            {
                var plant = int.Parse(Session["PlantID"].ToString());

                var user = new AuthUser();
                user.user_code = user_code;
                user.utype_id = utype_id;
                user.plant_id = plant;

                dbQIS.AuthUser.Add(user);
                dbQIS.SaveChanges();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public ActionResult EditUser(string user_code, string name, byte utype_id)
        {
            try
            {
                var query = from u in dbQIS.AuthUser
                            where u.user_code.Equals(user_code)
                            select u;
                var user = query.First();
                user.utype_id = utype_id;
                dbQIS.SaveChanges();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public ActionResult DeleteUser(string id)
        {
            try
            {
                var user = dbQIS.AuthUser.Find(id);
                dbQIS.AuthUser.Remove(user);
                dbQIS.SaveChanges();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        //-----------------------------------Product Master--------------------------------------//
        public ActionResult GridProductData(string sidx, string sord, int page, int rows, string search)
        {
            var plant = int.Parse(Session["PlantID"].ToString());
            var db = dbQIS.Master_Product.Where(p => p.plant_id == plant && p.product_del_flag == false);
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = db.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            // This is possible because I'm using the LINQ Dynamic Query Library
            //var products = db.OrderBy(sidx + " " + sord)
            //        .Skip(pageIndex * pageSize)
            //        .Take(pageSize).AsEnumerable();

            var products = (from p in db.ToList()
                            join g in dbTNC.tnc_group_master.ToList()
                            on p.group_id equals g.id
                            select new
                            {
                                product_id = p.product_id,
                                product_name = p.product_name,
                                plant_id = p.Master_Plant.plant_name,
                                group_id = g.group_name
                            }).AsQueryable()
                            .OrderBy(sidx + " " + sord)
                            .Skip(pageIndex * pageSize)
                            .Take(pageSize).AsEnumerable();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = products.Select(p => new
                {
                    i = p.product_id,
                    cell = new object[] {
                        p.product_id,
                        p.product_name,
                        p.plant_id,
                        p.group_id
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }

        public ActionResult AddProduct(string product_name, int group_id)
        {
            try
            {
                var plant = int.Parse(Session["PlantID"].ToString());

                var product = new Master_Product();
                product.product_name = product_name;
                product.plant_id = plant;
                product.group_id = group_id;
                product.product_del_flag = false;

                dbQIS.Master_Product.Add(product);
                dbQIS.SaveChanges();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public ActionResult EditProduct(int product_id, string product_name, int group_id)
        {
            try
            {
                var plant = int.Parse(Session["PlantID"].ToString());
                var query = from p in dbQIS.Master_Product
                            where p.product_id.Equals(product_id)
                            select p;
                var product = query.First();
                product.product_name = product_name;
                product.group_id = group_id;
                dbQIS.SaveChanges();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public ActionResult DeleteProduct(int id = 0)
        {
            try
            {
                var product = dbQIS.Master_Product.Find(id);
                //dbQIS.Master_Product.Remove(product);
                product.product_del_flag = true;
                dbQIS.SaveChanges();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        public string UTypeSelectList()
        {
            string result = "<select>";
            foreach (var item in dbQIS.Master_UType.Where(ut => ut.utype_id > 1))
            {
                result = result + "<option value = '" + item.utype_id + "'>" + item.utype_name + "</option>";
            }
            result = result + "</select>";
            return result;
        }

    }
}
