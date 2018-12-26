using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Neo4jClient.Cypher;
namespace FindAndRead.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            
            string movie= Cypher.Merge("(k:Knjiga{Naziv:'" + item.Naziv +
"',GodinaObjave:'" + item.GodinaObjave + "',Status:'Slobodna',Id:'" + item.Id + "'})")
.Return(k => k.As<Knjiga>()).Results.SingleOrDefault();
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}