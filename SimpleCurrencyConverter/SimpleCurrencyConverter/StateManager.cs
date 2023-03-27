using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
        private const string ExchangeViewStateFileName = "exchangeStateView.json";
        private const string ExchangeRatesFileName = "exchangeRates.json";

        private const string BaseApiAddress = "https://www.alexandernorup.com/";
        private const string ExchangeRatesApiEndpoint = "res/exchangeRates.php";
        private ExchangeRates m_exchangeRates;
        private Dictionary<string, JsonSalesTax> m_salesTax;

        public ExchangeViewState ExchangeViewState { get; set; } = new ExchangeViewState();

        public StateManager()
        {
            TryParseJsonFile<ExchangeViewState>(ExchangeViewStateFileName, out var viewState);
            if (viewState != null)
            {
                ExchangeViewState = viewState;
            }

            TryParseJsonFile(ExchangeRatesFileName, out m_exchangeRates);

            //Since sales tax is embedded, you have to read it another way. 
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

        public void SaveState()
        {
            var exchangeRatesPath = GetAccessibleFolderPath(ExchangeRatesFileName);
            var exchangeViewStatePath = GetAccessibleFolderPath(ExchangeViewStateFileName);

            File.WriteAllText(exchangeRatesPath, JsonConvert.SerializeObject(m_exchangeRates));
            File.WriteAllText(exchangeViewStatePath, JsonConvert.SerializeObject(ExchangeViewState));
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
                    SaveState();
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

        private bool TryParseJsonFile<T>(string filename, out T output) where T : class
        {
            output = null;
            var filePath = GetAccessibleFolderPath(filename);
            if (File.Exists(filePath))
            {
                var fileContents = File.ReadAllText(filePath);
                if (fileContents.Trim() != string.Empty)
                {
                    var parsedJson = JsonConvert.DeserializeObject<T>(fileContents);
                    if (parsedJson != null)
                    {
                        output = parsedJson;
                        return true;
                    }
                }
            }
            return false;
        }

        private string GetAccessibleFolderPath(string filename)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
        }
    }
}
