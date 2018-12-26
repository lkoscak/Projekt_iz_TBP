using FindAndRead.Models;
using FindAndRead.Neo4j;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace FindAndRead.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            Person actor = Neo4jConnectionHandler.Client.Cypher.Match("(m:Person)").Where((Person
                m) => m.name=="Tom Hanks").Return(m => m.As<Person>()).Results.Single();
            return View();

        }

        public String Save()
        {
            Person actor = Neo4jConnectionHandler.Client.Cypher.Match("(m:Person)").Where((Person
                m) => m.name == "Tom Hanks").Return(m => m.As<Person>()).Results.Single();
            //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
            return actor.name+" "+actor.born;
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