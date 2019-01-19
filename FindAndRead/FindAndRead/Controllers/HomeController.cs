using FindAndRead.Models;
using FindAndRead.Neo4j;
using Newtonsoft.Json;
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
            return View();
        }

        //Provjerava da li je korisnik ulogiran u sustav
        private String userLogedIn()
        {
            HttpCookie user_login_cookie = Request.Cookies["prijavljeni_korisnik"];
            if (user_login_cookie != null) return user_login_cookie["korisnicko_ime"];
            else return "notLogedIn";
        }

        //Kreira kolačić koji pamti prijavljenog korisnika
        private void createCookie(String korisnickoIme,String lozinka)
        {
            HttpCookie user = new HttpCookie("prijavljeni_korisnik");
            user["korisnicko_ime"] = korisnickoIme;
            Response.Cookies.Add(user);
        }

        //Metoda za prijavu
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

        // Vraća status login kolačića
        public String LoginStatus()
        {
            String response = "ne";
            HttpCookie user_login_cookie = Request.Cookies["prijavljeni_korisnik"];
            if (user_login_cookie != null)
            {
                response = user_login_cookie["korisnicko_ime"];
            }

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(response);

            return json;

        }

        // Odjavljuje korisnika iz sustava
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
 
        //Dohvat autora za popunjavanje izbornika
        public String GetAuthors()
        {
            
            IEnumerable<Autor> autori = Neo4jConnectionHandler.Client.Cypher.Match("(a:Pisac)").Return(a => a.As<Autor>()).Results.ToList().OrderBy(o => o.ime); ;
         
            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(autori);

            return json;

            

        }


        //Dohvat knjiga  za logiranog korisnika
        public List<BooksForTableData> getBooksForTableForLoggedInUser(String userName)
        {

            var query = Neo4jConnectionHandler.Client.Cypher.Match("(u:Korisnik)").Match("(b:Knjiga)").
                Match("(z:Korisnik)-[p:PROCITANO]->(b)-[:NAPISANO_OD]->(w:Pisac)").
                Where((Korisnik u) => u.korisnicko_ime == userName).AndWhere("NOT (u)-[:PROCITANO]->(b)")
               .Return((b, p, w) => new BooksForTableData
               {
                   knjiga = b.As<Book>(),
                   listaCitanja = p.CollectAs<ProcitanoVeza>(),
                   autor = w.As<Autor>()
               });


            var result = query.Results.ToList();
            return result;
        }

        //Općeniti dohvat knjiga za nelogiranog korisnika
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


        // Dohvat knjiga prema odabranom autoru za nelogiranog korisnika
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

        // Dohvat knjiga prema autoru za logiranog korisnika
        public List<BooksForTableData> getBooksForTableByAuthorForLogedUser(string autor, String userName)
        {
            var query = Neo4jConnectionHandler.Client.Cypher.Match("(u:Korisnik)").Match("(b:Knjiga)").
                Match("(z:Korisnik)-[p:PROCITANO]->(b)-[:NAPISANO_OD]->(w:Pisac)").Where((Autor w) => w.ime == autor).
                AndWhere((Korisnik u) => u.korisnicko_ime == userName).AndWhere("NOT (u)-[:PROCITANO]->(b)")
               .Return((b, p, w) => new BooksForTableData
               {
                   knjiga = b.As<Book>(),
                   listaCitanja = p.CollectAs<ProcitanoVeza>(),
                   autor = w.As<Autor>()
               });

            var result = query.Results.ToList();
            return result;
        }


        // Filtriranje dohvaćenih knjiga prema odabranom ratingu
        public string getBooksByRating(string rating)
        {
            List<BooksForTableData> result = null;
            if (userLogedIn() == "notLogedIn") result = getBooksForTable();
            else result = getBooksForTableForLoggedInUser(userLogedIn());

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

        // Izračuni prosječne ocjene i brojačitanja za knjige dohvaćene prema autoru
        public string GetAuthorsBooks(string autor)
        {
            List<BooksForTableData> result = null;
            if (userLogedIn() == "notLogedIn") result = getBooksForTableByAuthor(autor);
            else result = getBooksForTableByAuthorForLogedUser(autor,userLogedIn());


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

        // Filtrira knjigee prema odabranom vremenskom intervalu
        public string getBooksByTime(string timeInterval)
        {
            List<BooksForTableData> result = null;
            if (userLogedIn() == "notLogedIn") result = getBooksForTable();
            else result = getBooksForTableForLoggedInUser(userLogedIn());

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

        
        // Dohvat najboljih veza logiranog korisnika
        public List<TopVeze> getTopUsers(String userName)
        {
            var query = Neo4jConnectionHandler.Client.Cypher.Match("(u:Korisnik)").Match("(dr:Korisnik)").
                Match("(u:Korisnik)-[:PROCITANO]->(:Knjiga)<-[:PROCITANO]-(dr:Korisnik)").
                Where((Korisnik u) => u.korisnicko_ime == userName)
               .Return((dr) => new TopVeze
               {
                   Korisnik=dr.As<Korisnik>(),
                   BrojVeza=dr.Count()  
               });

            var res = query.Results.ToList();
            return res;
        }

        // Dohvat knjiga automatskim putem
        public List<BooksForTableData> getBooksForTableAutoWay()
        {
            String logedUser = userLogedIn();
            List<TopVeze> lista = getTopUsers(logedUser);
            lista.OrderByDescending(o => o.BrojVeza).ToList();
            List<BooksForTableData> listaKnjiga = new List<BooksForTableData>();
            while (listaKnjiga.Count() <= 5 && lista.Count != 0)
            {
                string topKorisnik = lista.First().Korisnik.ime;
                var query = Neo4jConnectionHandler.Client.Cypher.Match("(u:Korisnik)").
                    Match("(z:Korisnik)").
                    Match("(z)-[p:PROCITANO]->(b:Knjiga)-[:NAPISANO_OD]->(w:Pisac)").
                    Where((Korisnik u) => u.korisnicko_ime == logedUser).
                    AndWhere((Korisnik z) => z.ime== topKorisnik).
                    AndWhere("NOT (u)-[:PROCITANO]->(b)").

                   Return((b, p, w) => new BooksForTableData
                   {
                       knjiga = b.As<Book>(),
                       listaCitanja = p.CollectAs<ProcitanoVeza>(),
                       autor = w.As<Autor>()
                   });

                var result = query.Results.ToList();

                if (listaKnjiga.Count() == 0)
                {
                    foreach (var item in result)
                    {
                        setPropOfABook(item);
                        listaKnjiga.Add(item);
                    }
                }

                else
                {
                    bool flag = false;
                    foreach (var item in result)
                    {
                        foreach (var item1 in listaKnjiga)
                        {
                            if (item.knjiga.naslov == item1.knjiga.naslov)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag) flag = false;
                        else
                        {
                            setPropOfABook(item);
                            listaKnjiga.Add(item);
                        }
                    }
                }
                lista.RemoveAt(0);
            }

            return listaKnjiga;
           
        }

        // Vraća knjige dohvaćene automatskim putem
        public String GetAuto()
        {

            var json = JsonConvert.SerializeObject(getBooksForTableAutoWay());
            return json;
        }

        // Postavlja prosječnu ocjenu i broj čitanja za automatsku preporuku
        public void setPropOfABook(BooksForTableData knjiga)
        {
            var query = Neo4jConnectionHandler.Client.Cypher.OptionalMatch("(u:Korisnik)-[p:PROCITANO]->(b:Knjiga)").
                 Where((Book b) => b.naslov == knjiga.knjiga.naslov).Return(p => p.As<ProcitanoVeza>());

            var result = query.Results.ToList();

            knjiga.brojCitanja = result.Count();

            if (knjiga.brojCitanja == 0) knjiga.prosjecnaOcjena = 0;
            else
            {
                int sumaOcjena = 0;

                foreach (ProcitanoVeza veza in result.ToList())
                {
                    
     
                            sumaOcjena += veza.ocjena;
                       
                       

                }

                knjiga.prosjecnaOcjena = Math.Round((double)sumaOcjena / knjiga.brojCitanja, 2);

                }
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