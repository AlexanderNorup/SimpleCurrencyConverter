using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SimpleCurrencyConverter
{
    public class SalesTaxEntry
    {
        public string StateName { get; set; }
        public double Rate { get; set; }

        public SalesTaxEntry(string stateName, JsonSalesTax sale)
        {
            StateName = stateName;
            Rate = sale.Rate;
        }

        public override string ToString()
        {
            return StateName + " (" + Rate * 100d + "%)";
        }
    }
}
