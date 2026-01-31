using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Response
{
    public class FilterProductResponse
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public IEnumerable<DataResponse>? DataList { get; set; }

    }

    public class DataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrL { get; set; } = string.Empty;

        public List<VariantInfo> Variants { get; set; } = new List<VariantInfo>();


    }

    public class VariantInfo
    {
        public Guid Id { get; set; }
        public string Size { get; set; }
        public int Stock { get; set; }
    }
}

