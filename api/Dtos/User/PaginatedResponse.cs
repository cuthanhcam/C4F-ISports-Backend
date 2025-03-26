using System.Collections.Generic;

namespace api.Dtos.User
{
    public class PaginatedResponse<T>
    {
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
} 