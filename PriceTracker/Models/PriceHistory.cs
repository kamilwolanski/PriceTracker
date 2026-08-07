using System;
using PriceTracker.Features.PriceHistory.ValueObjects;

namespace PriceTracker.Models
{
    public class PriceHistory
    {
        public Guid Id { get; set; }
        public Money Price { get; set; }
        public DateTime CheckedAt { get; set; }
        public Guid TrackedProductId { get; set; }
        public TrackedProduct TrackedProduct { get; set; } = null!;
    }
}
