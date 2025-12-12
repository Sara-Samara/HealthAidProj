// PagedResult.cs
namespace HealthAidAPI.Helpers
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        public PagedResult(List<T> items, int totalCount)
        {
            Items = items;
            TotalCount = totalCount;
            
        }
    }
}

// Custom Exceptions
namespace HealthAidAPI.Exceptions
{
    public class AppointmentConflictException : Exception
    {
        public AppointmentConflictException(string message) : base(message) { }
    }

    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message) : base(message) { }
    }
}