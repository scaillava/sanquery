using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanQuery.Utils
{

    public class DBSchema : Attribute
    {
        public string schema { get; }
        public DBSchema(string Schema)
        {
            this.schema = Schema;
        }
    }
    //public class DBSchema : Attribute
    //{
    //    public string[] schemas { get; }
    //    public DBSchema(params string[] Schemas)
    //    {
    //        this.schemas = Schemas;
    //    }
    //}


    //public class DBOrder : Attribute
    //{
    //    public int order { get; }
    //    public DBOrder(int Order)
    //    {
    //        this.order = Order;
    //    }
    //}
}
