using SanQuery.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanQuery.Models
{
    [DBSchema("travel")]
    public class Travel 
    {
        public string destination { get; set; }
        public DateTime date { get; set; }
        public decimal kms { get; set; }
        public int carId { get; set; }
        public int driverId { get; set; }

        [DBSchema("driver")]
        public string lastName { get; set; }
        [DBSchema("driver")]
        public string firstName { get; set; }
        [DBSchema("car")]
        public int modelYear { get; set; }
        [DBSchema("car")]
        public string modelName { get; set; }



    }

    [DBSchema("travel")]
    public class TravelWith
    {
        public string destination { get; set; }
        public DateTime date { get; set; }
        public decimal kms { get; set; }
        public int carId { get; set; }
        public int driverId { get; set; }
        public Car car { get; set; }
        public Driver driver { get; set; }



    }

}
