using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Pagination
{
    public class PaginationResponse<T>
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int Count { get; set; }

        public IReadOnlyList<T> Data { get; set; }
    }
}
