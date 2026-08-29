using System;
using System.Collections.Generic;
using System.Text;
using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Tests.PriceHistory.ValueObjects
{
    public class CurrencyParserTests
    {
        [Fact]
        public void Parse_WithZloty_ReturnsMoneyInPLN()
        {
            var input = "123 zł";

            var result = CurrencyParser.Parse(input);

            Assert.Equal(123, result.Amount);
            Assert.Equal("PLN", result.CurrencyCode);
        }

        [Fact]
        public void TryParse_WithEmptyInput_ReturnsNull()
        {
            var input = "";

            var result = CurrencyParser.TryParse(input);
            Assert.Null(result);
        }

        [Fact]
        public void TryParse_WithNullInput_ReturnsNull()
        {
            string? input = null;

            var result = CurrencyParser.TryParse(input);
            Assert.Null(result);
        }

        [Fact]
        public void TryParse_WithInvalidInput_ReturnsNull()
        {
            var input = "abc";
            var result = CurrencyParser.TryParse(input);
            Assert.Null(result);
        }

        [Fact]
        public void Parse_WithEmptyInput_ThrowsArgumentException()
        {
            var input = "";
            Assert.Throws<ArgumentException>(() => CurrencyParser.Parse(input));
        }

        [Fact]
        public void Parse_WithValidPlnAmount_ReturnsMoney()
        {
            var input = "29 zł";
            var result = CurrencyParser.Parse(input);
            Assert.Equal(29, result.Amount);
            Assert.Equal("PLN", result.CurrencyCode);
        }

        [Fact]
        public void Parse_WithDollarSymbol_ReturnsUsd()
        {
            var input = "29 $";
            var result = CurrencyParser.Parse(input);
            Assert.Equal(29, result.Amount);
            Assert.Equal("USD", result.CurrencyCode);
        }

        [Fact]
        public void Parse_WithEuroSymbol_ReturnsEur()
        {
            var input = "29 €";
            var result = CurrencyParser.Parse(input);
            Assert.Equal(29, result.Amount);
            Assert.Equal("EUR", result.CurrencyCode);
        }

        [Fact]
        public void NormalizeCurrencyCode_WithPln_ReturnsPln()
        {
            var input = "PLN";

            var result = CurrencyParser.NormalizeCurrencyCode(input);

            Assert.Equal("PLN", result);
        }

        [Fact]
        public void NormalizeCurrencyCode_WithLowercaseUsd_ReturnsUsd()
        {
            var input = "usd";

            var result = CurrencyParser.NormalizeCurrencyCode(input);

            Assert.Equal("USD", result);
        }

        [Fact]
        public void NormalizeCurrencyCode_WithUnknownCurrency_ReturnsUppercaseCode()
        {
            var input = "jpy";

            var result = CurrencyParser.NormalizeCurrencyCode(input);

            Assert.Equal("JPY", result);
        }
    }
}
