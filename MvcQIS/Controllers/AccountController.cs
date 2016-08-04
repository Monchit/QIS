using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using MvcQIS.Filters;
using MvcQIS.Models;
using WebCommonFunction;

namespace MvcQIS.Controllers
{
    //[Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : Controller
    {
        TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();
        QISEntities dbQIS = new QISEntities();
        private TNCSecurity secure = new TNCSecurity();

        [AllowAnonymous]
        public ActionResult Login(string key = null)
        {
            if (key != null)
            {
                var username = secure.WebCenterDecode(key);
                var chklogin = secure.Login(username, "", false);

                if (chklogin != null)
                {
                    Session["QIS_Auth"] = chklogin.emp_code;
                    Session["FullName"] = chklogin.emp_fname + " " + chklogin.emp_lname;

                    var pcr_user = (from q in dbQIS.AuthUser
                                    where q.user_code == chklogin.emp_code
                                    select q).FirstOrDefault();
                    if (pcr_user != null)
                    {
                        Session["UserType"] = pcr_user.utype_id;
                        Session["PlantID"] = pcr_user.plant_id;
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return View();
            }
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(tnc_user model)
        {
            Util ut = new Util();
            var pass = ut.CalculateMD5Hash(model.password);

            if (ModelState.IsValid)
            {
                var chklogin = (from u in dbTNC.tnc_user
                                where u.username == model.username && u.password == pass
                                select u).FirstOrDefault();

                if (chklogin != null)
                {
                    Session["QIS_Auth"] = chklogin.emp_code;
                    Session["FullName"] = chklogin.emp_fname + " " + chklogin.emp_lname;

                    var pcr_user = (from q in dbQIS.AuthUser
                                    where q.user_code == model.username
                                    select q).FirstOrDefault();
                    if (pcr_user != null)
                    {
                        Session["UserType"] = pcr_user.utype_id;
                        Session["PlantID"] = pcr_user.plant_id;
                    }
                    return RedirectToAction("Index", "Home");
                }

                return View();
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //[HttpPost]
        //public ActionResult Login(string key = null)
        //{
        //    string username = key == null ? Request.Form["username"].ToString() : "";
        //    string pass = key == null ? Request.Form["password"].ToString() : "";

        //    var chklogin = secure.Login(username, pass, true);//set false to true for Real

        //    if (key != null)
        //    {
        //        username = secure.WebCenterDecode(key);
        //        chklogin = secure.Login(username, "a", false);
        //    }

        //    if (chklogin != null)//Login Success
        //    {
        //        Session["QIS_Auth"] = chklogin.emp_code;
        //        Session["FullName"] = chklogin.emp_fname + " " + chklogin.emp_lname;

        //        var pcr_user = (from q in dbQIS.AuthUser
        //                        where q.user_code == username
        //                        select q).FirstOrDefault();
        //        if (pcr_user != null)
        //        {
        //            Session["UserType"] = pcr_user.utype_id;
        //            Session["PlantID"] = pcr_user.plant_id;
        //        }
        //        return RedirectToAction("Index", "Home");
        //    }
        //    else
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }
        //}

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session.Remove("QIS_Auth");
            Session.Remove("UserType");
            Session.Remove("FullName");
            Session.Remove("PlantID");
            return RedirectToAction("Login","Account");
        }
    }
}
