using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Job;
using Job.DTO;
using Job.Helper;

namespace PEP.DataStatistics.Controllers
{
    public class DownloadController : Controller
    {
        //
        // GET: /Download/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RunTask()
        {
            new RunTask().Run();
            return View();
        }

        public ActionResult RunStat(string DateFrom, string DateEnd)
        {
            new StatService(DateFrom, DateEnd).Run();
            return View();
        }
	}
}