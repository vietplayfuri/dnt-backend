using System.Collections.Generic;

namespace dnt.core.Models.Response
{
    public class CollectionResponse<T> : OperationResponse
    {
        public IEnumerable<T> Items { get; set; }
    }
}
