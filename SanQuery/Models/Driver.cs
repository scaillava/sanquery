using SanQuery.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanQuery.Models
{
    [DBSchema("driver")]
    public class Driver
    {
        public int driverId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public int age { get; set; }
        
    }


    [DBSchema("driver")]
    public class DriverWithCar
    {
        public int driverId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public int age { get; set; }
        public List<Car> cars { get; set; }
    }
}
