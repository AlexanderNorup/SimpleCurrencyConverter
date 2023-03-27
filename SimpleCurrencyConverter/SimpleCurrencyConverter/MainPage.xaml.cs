using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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
            SalesTaxStatePicker.ItemsSource = salesTaxStates;
            SalesTaxStatePicker.SelectedIndexChanged += StateSelectionChanged;

            UpdateRatesBtn.Clicked += UpdateExchangeRatesClicked;
            InputValue.TextChanged += (o, e) => DoCalculations();
            InputCurrency.SelectedIndexChanged += (o, e) => { UpdateState(); DoCalculations(); };
            OutputCurrency.SelectedIndexChanged += (o, e) => { UpdateState(); DoCalculations(); };
            SalesTax.TextChanged += (o, e) => { UpdateState(); DoCalculations(); };
            UseSalesTax.CheckedChanged += (o, e) => { UpdateState(); DoCalculations(); };

            SwapCurrenciesBtn.Clicked += SwapCurrencies;
            NewInputBtn.Clicked += NewInput;

            _ = UpdateExchangeRates(false);
        }

        private void NewInput(object sender, EventArgs e)
        {
            InputValue.Text = string.Empty;
            InputValue.Focus();
        }

        private void SwapCurrencies(object sender, EventArgs e)
        {
            var inputCurrency = InputCurrency.SelectedIndex;
            var outputCurrency = OutputCurrency.SelectedIndex;
            var outputValueArr = OutputValue.Text.Split(' ');
            var outputValue = outputValueArr.Length > 1 ? outputValueArr[1] : string.Empty;
            var inputValie = InputValue.Text;

            InputValue.Text = outputValue;
            OutputValue.Text = inputValie;
            InputCurrency.SelectedIndex = outputCurrency;
            OutputCurrency.SelectedIndex = inputCurrency;

            DoCalculations();
            UpdateState();
        }

        private void UpdateState()
        {
            if (double.TryParse(SalesTax.Text, out var salesTax))
            {
                _stateManager.ExchangeViewState.LastSalesTaxValue = salesTax;
            }

            if (InputCurrency.SelectedItem is Currency inputCurrency)
            {
                _stateManager.ExchangeViewState.SelectedInputCurrency = inputCurrency;
            }

            if (OutputCurrency.SelectedItem is Currency outputCurrency)
            {
                _stateManager.ExchangeViewState.SelectedOutputCurrency = outputCurrency;
            }

            _stateManager.ExchangeViewState.UseSalesTax = UseSalesTax.IsChecked;

            _stateManager.SaveState();
        }

        private void ApplySavedState()
        {
            SalesTax.Text = _stateManager.ExchangeViewState.LastSalesTaxValue.ToString(CultureInfo.InvariantCulture);
            UseSalesTax.IsChecked = _stateManager.ExchangeViewState.UseSalesTax;

            InputCurrency.SelectedIndex = InputCurrency.ItemsSource.IndexOf(_stateManager.ExchangeViewState.SelectedInputCurrency);
            OutputCurrency.SelectedIndex = OutputCurrency.ItemsSource.IndexOf(_stateManager.ExchangeViewState.SelectedOutputCurrency);
        }

        private void DoCalculations()
        {
            if (!double.TryParse(InputValue.Text, out var inputValue))
            {
                OutputValue.Text = "";
                return;
            }

            var inputCurrency = InputCurrency.SelectedItem as Currency;
            if (inputCurrency is null)
            {
                OutputValue.Text = "";
                return;
            }

            var outputCurrency = OutputCurrency.SelectedItem as Currency;
            if (outputCurrency is null)
            {
                OutputValue.Text = "";
                return;
            }

            double salesTaxRate = 0;
            if (UseSalesTax.IsChecked && !double.TryParse(SalesTax.Text, out salesTaxRate))
            {
                OutputValue.Text = "! Invalid Sales Tax !";
                return;
            }

            var dkkInputValue = inputValue * (inputCurrency.ParsedRate / 100d);
            var salesTax = salesTaxRate * dkkInputValue;

            var dkkTotal = dkkInputValue + salesTax;

            var totalConvertedToOutput = dkkTotal;
            if (outputCurrency.Code != "DKK")
            {
                totalConvertedToOutput = dkkTotal / (outputCurrency.ParsedRate / 100d);
            }

            OutputValue.Text = outputCurrency.Code + " " + totalConvertedToOutput.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private async void UpdateExchangeRatesClicked(object sender, EventArgs e)
        {
            await UpdateExchangeRates(true);
        }

        private async Task UpdateExchangeRates(bool force)
        {
            ExchangeRateLbl.Text = "Fetching rates...";
            try
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

                var inputSelectedIndex = InputCurrency.SelectedIndex;
                var outputSelectedIndex = OutputCurrency.SelectedIndex;

                InputCurrency.ItemsSource = currencies;
                OutputCurrency.ItemsSource = currencies;

                InputCurrency.SelectedIndex = inputSelectedIndex;
                OutputCurrency.SelectedIndex = outputSelectedIndex;

                ExchangeRateLbl.Text = "Exchange rates from: " + ExchangeRates.EffectiveDate.ToLongDateString();
                ApplySavedState();
            }
            catch (Exception ex)
            {
                ExchangeRateLbl.Text = "Failed to fetch Exchange-rates: " + ex.Message;
            }
        }

        private void StateSelectionChanged(object sender, EventArgs e)
        {
            SalesTax.Text = (SalesTaxStatePicker.SelectedItem as SalesTaxEntry)?.Rate.ToString(CultureInfo.InvariantCulture) ?? "<unparseable item>";
            DoCalculations();
        }

        public ExchangeRates ExchangeRates { get; set; }
    }
}
