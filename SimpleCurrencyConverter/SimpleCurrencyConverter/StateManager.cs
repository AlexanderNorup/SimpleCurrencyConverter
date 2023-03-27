using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCurrencyConverter
{
    public class StateManager
    {
        private const string BaseApiAddress = "https://www.alexandernorup.com/";
        private const string ExchangeRatesApiEndpoint = "res/exchangeRates.php";
        private ExchangeRates m_exchangeRates;
        private Dictionary<string, JsonSalesTax> m_salesTax;

        public StateManager()
        {
            var salesTaxContent = ReadAssemblyJson("sales_tax_rates.json");
            var rawSalesTax = JsonConvert.DeserializeObject<Dictionary<string, JsonSalesTax>>(salesTaxContent);

            var filteredSalexTax = new Dictionary<string, JsonSalesTax>();
            foreach (var entry in rawSalesTax)
            {
                if (entry.Value.Type == "vat")
                {
                    filteredSalexTax.Add(entry.Key, entry.Value);
                }
            }
            m_salesTax = filteredSalexTax;
        }

        public async Task<ExchangeRates> GetExchangeRates()
        {
            if (m_exchangeRates is null)
            {
                // Should be a lock here for concurrency. But will this actually happen? Probably not. 
                return await ForceFetchExchangeRates();
            }
            return m_exchangeRates;
        }

        public async Task<ExchangeRates> ForceFetchExchangeRates()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseApiAddress);
                using (var response = await client.GetAsync(ExchangeRatesApiEndpoint))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    m_exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(await response.Content.ReadAsStringAsync());
                    return m_exchangeRates;
                }
            }
        }

        public Dictionary<string, JsonSalesTax> GetSalesTax()
        {
            return m_salesTax;
        }

        private string ReadAssemblyJson(string jsonFileName)
        {
            string jsonString;
            var assembly = typeof(StateManager).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{jsonFileName}");
            using (var reader = new System.IO.StreamReader(stream))
            {
                jsonString = reader.ReadToEnd();
            }
            return jsonString;
        }
    }
}
