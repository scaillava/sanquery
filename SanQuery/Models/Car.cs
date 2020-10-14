using SanQuery.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanQuery.Models
{
    [DBSchema("car")]
    public class Car
    {
        public int carId { get; set; }
        public int modelYear { get; set; }
        public string modelName { get; set; }
        public int driverId { get; set; }
        [DBSchema("driver")]
        public string lastName { get; set; }
        [DBSchema("driver")]
        public string firstName { get; set; }

    }

    [DBSchema("car")]
    public class CarWithDriver
    {
        public int carId { get; set; }
        public int modelYear { get; set; }
        public string modelName { get; set; }
        public int driverId { get; set; }
        public Driver driver { get; set; }
     

    }
}
