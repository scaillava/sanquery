using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanQuery.Models
{
    public class QueryModel
    {
        public string modelName { get; set; }
        public Type modelType { get; set; }
        public string queryFrom { get; set; }
    }
}
