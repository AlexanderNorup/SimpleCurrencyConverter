using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCurrencyConverter
{
    public class ExchangeRates
    {
        public DateTime EffectiveDate { get; set; }
        public string BaseCurrency { get; set; }
        public Dictionary<string, Currency> Currencies { get; set; }
    }
}
