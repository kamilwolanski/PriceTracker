using PriceTracker.Features.PriceHistory.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace PriceTracker.Tests.PriceHistory.ValueObjects
{
    public class MoneyTests
    {
        [Fact]
        public void Money_WithValidAmountAndCurrency_CreatesInstance()
        {
            var inputAmount = 20m;
            var inputCurrencyCode = "PLN";

            var money = new Money(inputAmount, inputCurrencyCode);

            Assert.Equal(20, money.Amount);
            Assert.Equal("PLN", money.CurrencyCode);
        }

        [Fact]
        public void Money_WithZeroAmount_ThrowsArgumentException()
        {
            var inputAmount = 0m;
            var inputCurrencyCode = "PLN";

            Assert.Throws<ArgumentException>(() => new Money(inputAmount, inputCurrencyCode));
        }

        [Fact]
        public void Money_WithNegativeAmount_ThrowsArgumentException()
        {
            var inputAmount = -10m;
            var inputCurrencyCode = "PLN";

            Assert.Throws<ArgumentException>(() => new Money(inputAmount, inputCurrencyCode));
        }

        [Fact]
        public void Money_WithNullCurrencyCode_ThrowsArgumentException()
        {
            var inputAmount = 10;
            string? inputCurrencyCode = null;

            Assert.Throws<ArgumentException>(() => new Money(inputAmount, inputCurrencyCode));
        }

        [Fact]
        public void Money_WithEmptyCurrencyCode_ThrowsArgumentException()
        {
            var inputAmount = 10;
            string inputCurrencyCode = "";

            Assert.Throws<ArgumentException>(() => new Money(inputAmount, inputCurrencyCode));
        }

        [Fact]
        public void Money_WithCurrencyCodeLongerThanThreeCharacters_ThrowsArgumentException()
        {
            var inputAmount = 10;
            string inputCurrencyCode = "PLNW";

            Assert.Throws<ArgumentException>(() => new Money(inputAmount, inputCurrencyCode));
        }

        [Fact]
        public void Money_WithCurrencyCodeShorterThanThreeCharacters_ThrowsArgumentException()
        {
            var inputAmount = 10;
            string inputCurrencyCode = "PL";

            Assert.Throws<ArgumentException>(() => new Money(inputAmount, inputCurrencyCode));
        }

        [Fact]
        public void Money_WithLowercaseCurrencyCode_NormalizesToUppercase()
        {
            var inputAmount = 10m;
            string inputCurrencyCode = "pln";

            var money = new Money(inputAmount, inputCurrencyCode);

            Assert.Equal(10m, money.Amount);
            Assert.Equal("PLN", money.CurrencyCode);
        }
    }
}
