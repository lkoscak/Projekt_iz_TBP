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

        private void createCookie(String korisnickoIme,String lozinka)
        {
            HttpCookie user = new HttpCookie("prijavljeni_korisnik");
            user["korisnicko_ime"] = korisnickoIme;
            Response.Cookies.Add(user);
        }

        public String prijava(String korIme, String lozinka)
        {
            Korisnik korisnik = Neo4jConnectionHandler.Client.Cypher.OptionalMatch("(a:Korisnik)").Where((Korisnik
               a) => a.korisnicko_ime == korIme).AndWhere((Korisnik a)=>a.lozinka==lozinka).Return(a => a.As<Korisnik>()).Results.Single();

            var jsonSerialiser = new JavaScriptSerializer();
            if (korisnik != null)
            {
                createCookie(korIme, lozinka);
            }

            var json = jsonSerialiser.Serialize(korisnik);

            return json;
        }

        public String GetAuthors()
        {
            /*Autor autor = Neo4jConnectionHandler.Client.Cypher.Match("(a:Autor)").Where((Autor
                a) => a.ime == "Tom Hanks").Return(m => m.As<Person>()).Results.Single();
            //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
            return actor.name+" "+actor.born;*/

            IEnumerable<Autor> autori = Neo4jConnectionHandler.Client.Cypher.Match("(a:Pisac)").Return(a => a.As<Autor>()).Results.ToList().OrderBy(o => o.ime); ;
         
            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(autori);

            return json;

        }

        public List<BooksForTableData> getBooksForTable()
        {
            var query = Neo4jConnectionHandler.Client.Cypher.OptionalMatch("(u:Korisnik)-[p:PROCITANO]->(b:Knjiga)-[n:NAPISANO_OD]->(w:Pisac)")
               .Return((b, p, w) => new BooksForTableData
               {
                   knjiga = b.As<Book>(),
                   listaCitanja = p.CollectAs<ProcitanoVeza>(),
                   autor = w.As<Autor>()
               });

            var result = query.Results.ToList();
            return result;
        }

        public List<BooksForTableData> getBooksForTableByAuthor(string autor)
        {
            var query = Neo4jConnectionHandler.Client.Cypher.OptionalMatch("(u:Korisnik)-[p:PROCITANO]->(b:Knjiga)-[n:NAPISANO_OD]->(w:Pisac)").Where((Autor w)=>w.ime==autor)
               .Return((b, p, w) => new BooksForTableData
               {
                   knjiga = b.As<Book>(),
                   listaCitanja = p.CollectAs<ProcitanoVeza>(),
                   autor = w.As<Autor>()
               });

            var result = query.Results.ToList();
            return result;
        }

        public string getBooksByRating(string rating)
        {
            List<BooksForTableData> result = getBooksForTable();

            foreach (BooksForTableData booksByRatingData in result.ToList())
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

                if (booksByRatingData.prosjecnaOcjena < int.Parse(rating)) result.Remove(booksByRatingData);


            }

            List<BooksForTableData> sortedList = result.OrderByDescending(o => o.prosjecnaOcjena).ToList();

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(sortedList);

            return json;

        }

        public string GetAuthorsBooks(string autor)
        {
            List<BooksForTableData> result = getBooksForTableByAuthor(autor);

            foreach (BooksForTableData booksByAuthor in result.ToList())
            {
                booksByAuthor.brojCitanja = booksByAuthor.listaCitanja.Count();
                if (booksByAuthor.brojCitanja == 0) booksByAuthor.prosjecnaOcjena = 0;
                else
                {
                    int sumaOcjena = 0;
                    foreach (ProcitanoVeza procitanoVeza in booksByAuthor.listaCitanja)
                    {
                        sumaOcjena += procitanoVeza.ocjena;
                    }

                    booksByAuthor.prosjecnaOcjena = Math.Round((double)sumaOcjena / booksByAuthor.brojCitanja, 2);

                }




            }

            List<BooksForTableData> sortedList = result.OrderByDescending(o => o.prosjecnaOcjena).ToList();

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(sortedList);

            return json;

        }

        public String LoginStatus()
        {
            String response = "ne";
            HttpCookie user_login_cookie = Request.Cookies["prijavljeni_korisnik"];
            if (user_login_cookie != null)
            {
                response = "da";
            }

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(response);

            return json;

        }

        public void odjava()
        {
            HttpCookie user_login_cookie = Request.Cookies["prijavljeni_korisnik"];
            if (user_login_cookie != null)
            {
                HttpCookie myCookie = new HttpCookie("prijavljeni_korisnik");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
        }

        public string getBooksByTime(string timeInterval)
        {
            List<BooksForTableData> result = getBooksForTable();

            DateTime trenutniDatum = DateTime.Now.Date;
            bool readFlag=false;
            switch (timeInterval)
            {
                case("tjedan"):
                    { trenutniDatum = trenutniDatum.AddDays(-7); break; }
                case ("mjesec"):
                    { trenutniDatum = trenutniDatum.AddDays(-30); break; }
                case ("godina"):
                    { trenutniDatum = trenutniDatum.AddDays(-365); break; }
                case ("sve"):
                    { trenutniDatum = trenutniDatum.AddYears(-100); break; }
                default:
                    break;
            }

            foreach (BooksForTableData booksByRatingData in result.ToList())
            {
                readFlag = false;
                //booksByRatingData.brojCitanja = booksByRatingData.listaCitanja.Count();
                //if (booksByRatingData.brojCitanja == 0) booksByRatingData.prosjecnaOcjena = 0;
                int sumaOcjena = 0;
                int brojCitanja = 0;
                    foreach (ProcitanoVeza procitanoVeza in booksByRatingData.listaCitanja)
                    {
                        
                        DateTime bookDate = DateTime.ParseExact(procitanoVeza.datum, "dd.MM.yyyy.", System.Globalization.CultureInfo.InvariantCulture);

                    if (bookDate >= trenutniDatum)
                    {
                        readFlag = true;
                        brojCitanja++;
                        sumaOcjena += procitanoVeza.ocjena;
                    }
                    }

                if (!readFlag) result.Remove(booksByRatingData);
                else
                {
                    booksByRatingData.brojCitanja = brojCitanja;
                    booksByRatingData.prosjecnaOcjena = Math.Round((double)sumaOcjena / brojCitanja, 2);
                }
      
            }

            List<BooksForTableData> sortedList = result.OrderByDescending(o => o.brojCitanja).ToList();

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