using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tasks.ActionFilters
{
    public class LogFilter: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Mesaj(filterContext.HttpContext.Request.Url+ " " +filterContext.HttpContext.Request.UserHostAddress
                + " " + filterContext.HttpContext.Request.UserAgent);
        }
        
        private void Mesaj(string mesaj)
        {
            Debug.WriteLine(mesaj+ DateTime.Now);
        }
    }
}