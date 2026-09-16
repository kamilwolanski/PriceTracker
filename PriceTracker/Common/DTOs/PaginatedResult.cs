namespace PriceTracker.Common.DTOs
{
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = [];
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
