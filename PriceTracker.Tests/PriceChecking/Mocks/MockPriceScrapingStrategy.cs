using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.PriceHistory.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace PriceTracker.Tests.PriceChecking.Mocks
{
    public class MockPriceScrapingStrategy : IPriceScrapingStrategy
    {
        public int Priority { get; set; } = 1;
        public bool Handle { get; set; } = true;
        public Money? Price { get; set; } = new Money(20, "PLN");

        public bool CanHandle(Uri uri)
        {
            return Handle;
        }

        public Task<Money?> ScrapePriceAsync(
            Uri uri,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Price);
        }
    }
}
