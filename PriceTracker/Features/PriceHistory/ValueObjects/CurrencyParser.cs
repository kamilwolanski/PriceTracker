using System.Globalization;
using System.Text.RegularExpressions;

namespace PriceTracker.Features.PriceHistory.ValueObjects
{
    public static class CurrencyParser
    {
        private static readonly Dictionary<string, string> CurrencyMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "z³", "PLN" },
            { "zl", "PLN" },
            { "zlotych", "PLN" },
            { "z³oty", "PLN" },
            { "pln", "PLN" },
            { "$", "USD" },
            { "usd", "USD" },
            { "€", "EUR" },
            { "eur", "EUR" },
            { "L", "GBP" },
            { "gbp", "GBP" },
            { "chf", "CHF" }
        };

        public static Money Parse(string rawInput, string defaultCurrencyCode = "PLN")
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                throw new ArgumentException("Tekst do parsowania nie mo¿e byæ pusty.", nameof(rawInput));

            var amount = ParseAmount(rawInput);
            var currencyCode = TryExtractCurrencyCode(rawInput) ?? NormalizeCurrencyCode(defaultCurrencyCode);

            return new Money(amount, currencyCode);
        }

        public static Money? TryParse(string? rawInput, string defaultCurrencyCode = "PLN")
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return null;

            try
            {
                return Parse(rawInput, defaultCurrencyCode);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public static string NormalizeCurrencyCode(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                return "PLN";

            var trimmed = currency.Trim();
            return CurrencyMap.TryGetValue(trimmed, out var mapped)
                ? mapped
                : trimmed.ToUpperInvariant();
        }

        private static decimal ParseAmount(string rawInput)
        {
            var cleaned = Regex.Replace(rawInput, @"[^\d,.]", "");
            if (string.IsNullOrWhiteSpace(cleaned))
                throw new FormatException($"Nie uda³o siê odczytaæ kwoty z tekstu: '{rawInput}'");

            var commaIndex = cleaned.LastIndexOf(',');
            var dotIndex = cleaned.LastIndexOf('.');

            if (commaIndex >= 0 && dotIndex >= 0)
            {
                cleaned = commaIndex > dotIndex
                    ? cleaned.Replace(".", "").Replace(',', '.')
                    : cleaned.Replace(",", "");
            }
            else if (commaIndex >= 0)
            {
                cleaned = NormalizeSingleSeparator(cleaned, commaIndex, ',');
            }
            else if (dotIndex >= 0)
            {
                cleaned = NormalizeSingleSeparator(cleaned, dotIndex, '.');
            }

            if (!decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
                throw new FormatException($"Niepoprawny format liczby: '{cleaned}'");

            return amount;
        }

        private static string? TryExtractCurrencyCode(string rawInput)
        {
            foreach (var entry in CurrencyMap)
            {
                if (rawInput.Contains(entry.Key, StringComparison.OrdinalIgnoreCase))
                    return entry.Value;
            }

            var isoMatch = Regex.Match(rawInput, @"\b[A-Z]{3}\b", RegexOptions.IgnoreCase);
            return isoMatch.Success ? isoMatch.Value.ToUpperInvariant() : null;
        }

        private static string NormalizeSingleSeparator(string value, int separatorIndex, char separator)
        {
            var digitsAfterSeparator = value.Length - separatorIndex - 1;
            if (digitsAfterSeparator == 3)
                return value.Replace(separator.ToString(), "");

            return separator == ','
                ? value.Replace(',', '.')
                : value;
        }
    }
}
