using MvcQIS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using MvcJqGrid;
using System.Data.Entity;

namespace MvcQIS.Controllers
{
    public class AdminController : Controller
    {
        QISEntities dbQIS = new QISEntities();
        TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();

        //
        // GET: /Admin/

        public ActionResult User_Master()
        {
            ViewBag.Title = "User Master";
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (Session["UserType"].ToString() != "0")// 0 = Admin
                    return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public ActionResult Plant_Master()
        {
            ViewBag.Title = "Plant Master";
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (Session["UserType"].ToString() != "0")// 0 = Admin
                    return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public ActionResult Product_Master()
        {
            ViewBag.Title = "Product Master";
            if (Session["QIS_Auth"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if (Session["UserType"].ToString() != "0")// 0 = Admin
                    return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //-----------------------------------User Master--------------------------------------//
        public ActionResult GridUserData(string sidx, string sord, int page, int rows, string search)
        {
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbQIS.AuthUser.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            var users = (from u in dbQIS.AuthUser.ToList()
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
        public ActionResult AddUser(string user_code, byte utype_id, int plant_id)
        {
            try
            {
                var db = new QISEntities();

                var user = new AuthUser();
                user.user_code = user_code;
                user.utype_id = utype_id;
                user.plant_id = plant_id;

                db.AuthUser.Add(user);
                db.SaveChanges();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }
        public ActionResult EditUser(string user_code, byte utype_id, int plant_id)
        {
            try
            {
                var db = new QISEntities();
                var query = from u in db.AuthUser
                            where u.user_code.Equals(user_code)
                            select u;
                var user = query.First();
                user.utype_id = utype_id;
                user.plant_id = plant_id;
                db.SaveChanges();
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
                var db = new QISEntities();
                var user = db.AuthUser.Find(id);
                db.AuthUser.Remove(user);
                db.SaveChanges();
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
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbQIS.Master_Product.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);
            
            var products = (from p in dbQIS.Master_Product.Where(w => w.product_del_flag == false).ToList()
                            join g in dbTNC.tnc_group_master.ToList()
                            on p.group_id equals g.id
                            select new{
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
        public ActionResult AddProduct(string product_name, int plant_id, int group_id)
        {
            try
            {
                var db = new QISEntities();

                var product = new Master_Product();
                product.product_name = product_name;
                product.plant_id = plant_id;
                product.group_id = group_id;
                product.product_del_flag = false;

                db.Master_Product.Add(product);
                db.SaveChanges();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }
        public ActionResult EditProduct(int product_id, string product_name, int plant_id, int group_id)
        {
            try
            {
                var db = new QISEntities();
                var query = from p in db.Master_Product
                            where p.product_id.Equals(product_id)
                            select p;
                var product = query.First();
                product.product_name = product_name;
                product.plant_id = plant_id;
                product.group_id = group_id;
                db.SaveChanges();
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
                var db = new QISEntities();
                var product = db.Master_Product.Find(id);
                //db.Master_Product.Remove(product);
                product.product_del_flag = true;
                db.SaveChanges();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        //-----------------------------------Plant Master--------------------------------------//
        public ActionResult GridPlantData(string sidx, string sord, int page, int rows, string search)
        {
            var pageIndex = Convert.ToInt32(page) - 1;
            var pageSize = rows;
            var totalRecords = dbQIS.Master_Plant.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (float)pageSize);

            // This is possible because I'm using the LINQ Dynamic Query Library
            var plants = dbQIS.Master_Plant
                    .Where(w => w.plant_del_flag == false)
                    .OrderBy(sidx + " " + sord)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize).AsEnumerable();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = totalRecords,
                rows = plants.Select(p => new
                {
                    i = p.plant_id,
                    cell = new object[] {
                        p.plant_id,
                        p.plant_name
                    }
                }).ToArray()
            };

            return Json(jsonData);
        }
        public ActionResult AddPlant(string plant_name)
        {
            try
            {
                var db = new QISEntities();
                Master_Plant mp = new Master_Plant();
                mp.plant_name = plant_name;
                mp.plant_del_flag = false;
                db.Master_Plant.Add(mp);
                db.SaveChanges();

                return Json(true);
            }
            catch (Exception)
            {
                // Do some error logging stuff, handle exception, etc.
                return Json(false);
            }
        }
        public ActionResult EditPlant(int plant_id, string plant_name)
        {
            try
            {
                var db = new QISEntities();

                var query = from u in db.Master_Plant
                            where u.plant_id.Equals(plant_id)
                            select u;

                var plant = query.First();
                plant.plant_name = plant_name;
                db.SaveChanges();

                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }
        public ActionResult DeletePlant(int id = 0)
        {
            try
            {
                var db = new QISEntities();

                var plant = db.Master_Plant.Find(id);
                /*from u in db.Master_Plant
                        where u.plant_id.Equals(plant_id)
                        select u;
                var plant = query.FirstOrDefault();*/
                //db.Master_Plant.Remove(plant);
                plant.plant_del_flag = true;
                db.SaveChanges();
                return Json(true);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        //-----------------------------------Select List--------------------------------------//
        public string UserSelectList()
        {
            TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();

            string result = "<select>";

            foreach (var item in dbTNC.tnc_user.OrderBy(u => u.emp_fname))
            {
                result = result + "<option value = '" + item.emp_code + "'>" + item.emp_fname + " " + item.emp_lname + "</option>";
            }

            result = result + "</select>";
            return result;
        }
        public string GroupSelectList()
        {
            TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();

            string result = "<select>";

            foreach (var item in dbTNC.tnc_group_master.OrderBy(g => g.group_name))
            {
                result = result + "<option value = '" + item.id + "'>" + item.group_name + "</option>";
            }

            result = result + "</select>";
            return result;
        }
        public string PlantSelectList()
        {
            QISEntities dbQIS = new QISEntities();

            string result = "<select>";

            foreach (var item in dbQIS.Master_Plant)
            {
                result = result + "<option value = '" + item.plant_id + "'>" + item.plant_name + "</option>";
            }

            result = result + "</select>";
            return result;
        }
        public string UTypeSelectList()
        {
            QISEntities dbQIS = new QISEntities();

            string result = "<select>";

            foreach (var item in dbQIS.Master_UType)
            {
                result = result + "<option value = '" + item.utype_id + "'>" + item.utype_name + "</option>";
            }

            result = result + "</select>";
            return result;
        }
    }
}
