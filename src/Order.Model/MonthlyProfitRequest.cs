using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Model
{
    public class MonthlyProfitRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Profit { get; set; }

        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM", new CultureInfo("en-UK"));
    }
}
