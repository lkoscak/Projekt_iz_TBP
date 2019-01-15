using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FindAndRead.Models
{
    public class BooksForTableData
    {
        public int brojCitanja { get; set; }
        public double prosjecnaOcjena { get; set; }
        public IEnumerable<ProcitanoVeza> listaCitanja { get; set; }
        public Book knjiga { get; set; }
        public Autor autor { get; set; }
    }
}