using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanQuery.ViewModels
{
    public class Sort
    {
        public enum SortEnum { asc, desc }
        public string column { get; set; }
        public SortEnum sortType { get; set; }

    }
}
