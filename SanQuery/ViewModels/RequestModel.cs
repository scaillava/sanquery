using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SanQuery.ViewModels
{
    public class RequestModel
    {
        [Required]
        public string model { get; set; }
        public string[] format { get; set; }
        public dynamic filter { get; set; }
        public Sort order { get; set; }
    }


    public class Or
    {

    }
    public class And
    {

    }
    public class Op
    {
        public dynamic equal { get; set; }
        public dynamic like { get; set; }
        public dynamic min { get; set; }
        public dynamic max { get; set; }
        public dynamic not { get; set; }
    }




}
