using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace viva.scraping.Models
{
    public class Cards
    {
        public List<Card> cards { get; set; }
    }

    public class Card
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
