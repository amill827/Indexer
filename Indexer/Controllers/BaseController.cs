using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Serilog;

namespace Indexer.Controllers
{
    public class BaseController : Controller
    {
        public BaseController() : base() { }

        public ActionResult GetEnvironmentMessage()
        {
            string message = Properties.Settings.Default.EnvironmentMessage;

            return Content(message);
        }
    }
}