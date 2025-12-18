using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class LienHeController : Controller
    {
        //
        // GET: /LienHe/
        public ActionResult Index()
        {
            ViewBag.HTSV = "Nguyen Duc Minh Tam";
            ViewBag.MSSV = "2001230776";
            return View();
        }
	}
}