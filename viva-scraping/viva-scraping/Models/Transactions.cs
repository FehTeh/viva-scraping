using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace viva.scraping.Models
{
    public class Transactions
    {
        public List<Transaction> transactions { get; set; }
    }

    public class Transaction
    {
        public string title { get; set; }
        public string amount { get; set; }
        public string charged_in { get; set; }
        public string charged_in_place { get; set; }
        public string validated_in { get; set; }
        public string validated_in_place { get; set; }
        public string expires_in { get; set; }
    }
}
