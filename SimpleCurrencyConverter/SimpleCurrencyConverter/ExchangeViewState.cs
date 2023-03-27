namespace SimpleCurrencyConverter
{
    public class ExchangeViewState
    {
        public Currency SelectedInputCurrency { get; set; }
        public Currency SelectedOutputCurrency { get; set; }
        public double LastSalesTaxValue { get; set; }
        public bool UseSalesTax { get; set; }
    }
}