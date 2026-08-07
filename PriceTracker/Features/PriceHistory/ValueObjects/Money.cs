namespace PriceTracker.Features.PriceHistory.ValueObjects
{
    public readonly record struct Money
    {
        public decimal Amount { get; }
        public string CurrencyCode { get; }

        public Money(decimal amount, string currencyCode)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Kwota musi byæ wiêksza od 0.", nameof(amount));
            }

            if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
            {
                throw new ArgumentException("Kod waluty musi byæ 3-literowym kodem ISO.", nameof(currencyCode));
            }

            Amount = amount;
            CurrencyCode = currencyCode.ToUpperInvariant();
        }
    }
}
