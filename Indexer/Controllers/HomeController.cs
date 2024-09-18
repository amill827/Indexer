using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Indexer.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            Log.Information("Test");

            return View();
        }
    }
}