using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FindAndRead.Models
{
    public class Korisnik
    {
        public string ime { get; set; }
        public int godine { get; set; }
        public string korisnicko_ime { get; set; }
        public string lozinka { get; set; }
    }
}