using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SimpleCurrencyConverter
{
    public class Currency
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public string Rate { get; set; }
        public double ParsedRate => GetParsedRate();

        private double? cachedParseRate = null;
        private double GetParsedRate()
        {
            if (cachedParseRate is null)
            {
                if (double.TryParse(Rate, out var r))
                {
                    cachedParseRate = r;
                }
                cachedParseRate = -1;
            }
            return cachedParseRate.Value;
        }

        public override string ToString()
        {
            return Desc + " (" + ParsedRate.ToString(CultureInfo.InvariantCulture) + ")";
        }
    }
}
