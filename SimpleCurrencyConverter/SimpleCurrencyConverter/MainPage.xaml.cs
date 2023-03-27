using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SimpleCurrencyConverter
{
    public partial class MainPage : ContentPage
    {
        private StateManager _stateManager;

        public MainPage()
        {
            InitializeComponent();
            _stateManager = new StateManager();
            var salesTaxStates = _stateManager.GetSalesTax().Select(x => new SalesTaxEntry(x.Key, x.Value)).OrderBy(x => x.StateName).ToList();
            //ExchangeRates = _stateManager.GetExchangeRates();
            SalesTaxStatePicker.ItemsSource = salesTaxStates;
            SalesTaxStatePicker.SelectedIndexChanged += StateSelectionChanged;

            UpdateRatesBtn.Clicked += UpdateExchangeRatesClicked;
        }

        private async void UpdateExchangeRatesClicked(object sender, EventArgs e)
        {
            await UpdateExchangeRates(true);
        }

        private async Task UpdateExchangeRates(bool force)
        {
            if (force)
            {
                ExchangeRates = await _stateManager.ForceFetchExchangeRates();
            }
            else
            {
                ExchangeRates = await _stateManager.GetExchangeRates();
            }

            var currencies = ExchangeRates.Currencies.Select(x => x.Value).ToList();
            currencies.Add(new Currency()
            {
                Desc = "Danske Kroner",
                Code = "DKK",
                Rate = "100.0"
            });
            InputCurrency.ItemsSource = currencies;
            OutputCurrency.ItemsSource = currencies;
        }

        private void StateSelectionChanged(object sender, EventArgs e)
        {
            SalesTax.Text = (SalesTaxStatePicker.SelectedItem as SalesTaxEntry).Rate.ToString(CultureInfo.InvariantCulture);
        }


        public ExchangeRates ExchangeRates { get; set; }
    }
}
