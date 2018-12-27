using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FindAndRead.Models
{
    public class BooksByRatingData
    {
        public int brojCitanja { get; set; }
        public double prosjecnaOcjena { get; set; }
        public IEnumerable<ProcitanoVeza> listaCitanja { get; set; }
        public Book knjiga { get; set; }
    }
}