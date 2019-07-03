using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ProductionTracker.Web.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            //User.Identity.IsAuthenticated
            //User.IsInRole("Admin");

            //FormsAuthentication.SetAuthCookie( , true);

            return View();
        }
    }
}