using SanQuery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanQuery.Utils
{
    public static class DAL
    {
        public static async Task<List<QueryModel>> getQueryModels()
        {
            List<QueryModel> queryModels = new List<QueryModel> {
                    new QueryModel()
                {
                        modelName="car", modelType=typeof(Car), queryFrom="car car inner join driver driver on car.driverId = driver.driverId"
                },
                     new QueryModel()
                {
                        modelName="car2", modelType=typeof(CarWithDriver), queryFrom="car car inner join driver driver on car.driverId = driver.driverId"
                }
                    ,
                    new QueryModel()
                {
                        modelName="driver", modelType=typeof(Driver), queryFrom="driver driver"
                } ,
                    new QueryModel()
                {
                        modelName="driver2", modelType=typeof(DriverWithCar), queryFrom="car car inner join driver driver on car.driverId = driver.driverId"
                }
                    ,
                    new QueryModel()
                {
                        modelName="travel", modelType=typeof(Travel), queryFrom="travel travel inner join car car on car.carId = travel.carId " +
                                                                                 "inner join driver driver on driver.driverId = travel.driverId"
                },
                       new QueryModel()
                {
                        modelName="travel2", modelType=typeof(TravelWith), queryFrom="travel travel inner join car car on car.carId = travel.carId " +
                                                                                 "inner join driver driver on driver.driverId = travel.driverId"
                }
            };
            return await Task.FromResult(queryModels);
        }


        //var q = from t in Assembly.GetExecutingAssembly().GetTypes()
        //        where t.IsClass && t.Namespace == nspace
        //        select t;
        //q.ToList().ForEach(t => Console.WriteLine(t.Name));
        //string query = "select "++ from { 1}"
        //    string.Format

    }
}
