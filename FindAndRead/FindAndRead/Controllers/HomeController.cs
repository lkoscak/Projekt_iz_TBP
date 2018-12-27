using FindAndRead.Models;
using FindAndRead.Neo4j;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace FindAndRead.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            /*Person actor = Neo4jConnectionHandler.Client.Cypher.Match("(m:Person)").Where((Person
                m) => m.name=="Tom Hanks").Return(m => m.As<Person>()).Results.Single();*/
            return View();

        }

        public String GetAuthors()
        {
            /*Autor autor = Neo4jConnectionHandler.Client.Cypher.Match("(a:Autor)").Where((Autor
                a) => a.ime == "Tom Hanks").Return(m => m.As<Person>()).Results.Single();
            //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
            return actor.name+" "+actor.born;*/

            IEnumerable<Autor> autori = Neo4jConnectionHandler.Client.Cypher.Match("(a:Pisac)").Return(a => a.As<Autor>()).Results.ToList();
            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(autori);

            return json;

        }

        public string getBooksByRating(string rating)
        {
             var query = Neo4jConnectionHandler.Client.Cypher.OptionalMatch("(u:Korisnik)-[p:PROCITANO]->(b:Knjiga)")
                .Return((b, p) => new BooksByRatingData
                {
                    knjiga=b.As<Book>(),
                    listaCitanja=p.CollectAs<ProcitanoVeza>()
                });

            var result = query.Results.ToList();

            foreach (BooksByRatingData booksByRatingData in result.ToList())
            {
                booksByRatingData.brojCitanja = booksByRatingData.listaCitanja.Count();
                if (booksByRatingData.brojCitanja == 0) booksByRatingData.prosjecnaOcjena = 0;
                else {
                    int sumaOcjena = 0;
                    foreach (ProcitanoVeza procitanoVeza in booksByRatingData.listaCitanja)
                    {
                        sumaOcjena += procitanoVeza.ocjena;
                    }

                    booksByRatingData.prosjecnaOcjena=Math.Round((double)sumaOcjena/booksByRatingData.brojCitanja,2);

                }

                if (booksByRatingData.prosjecnaOcjena < int.Parse("3")) result.Remove(booksByRatingData);


            }

            List<BooksByRatingData> sortedList = result.OrderByDescending(o => o.prosjecnaOcjena).ToList();

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(sortedList);

            return json;

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