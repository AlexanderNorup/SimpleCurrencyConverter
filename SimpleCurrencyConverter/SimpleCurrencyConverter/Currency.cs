﻿using System;
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
                if (double.TryParse(Rate, NumberStyles.Any, CultureInfo.InvariantCulture, out var r))
                {
                    cachedParseRate = r;
                }
                else
                {
                    cachedParseRate = 0;
                }
            }
            return cachedParseRate.Value;
        }

        public override string ToString()
        {
            return Desc + " (" + ParsedRate.ToString() + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj is Currency other)
            {
                return other.Code == Code;
            }
            return false;
        }

        public override int GetHashCode()
        {
            //Autogenerated
            int hashCode = -583508647;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Code);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Desc);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Rate);
            hashCode = hashCode * -1521134295 + ParsedRate.GetHashCode();
            hashCode = hashCode * -1521134295 + cachedParseRate.GetHashCode();
            return hashCode;
        }
    }
}
