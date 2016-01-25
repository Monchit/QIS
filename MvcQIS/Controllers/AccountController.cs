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

namespace MvcQIS.Controllers
{
    //[Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : Controller
    {
        TNC_ADMINEntities dbTNC = new TNC_ADMINEntities();
        QISEntities dbQIS = new QISEntities();
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login()
        {
            ViewBag.Message = "Login";
            return View();
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
                var user = (from u in dbTNC.tnc_user
                         where u.username == model.username && u.password == pass
                         select u).FirstOrDefault();

                if (user != null)
                {
                    Session["QIS_Auth"] = user.emp_code;
                    Session["FullName"] = user.emp_fname + " " + user.emp_lname;

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
