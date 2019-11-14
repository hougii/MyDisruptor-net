using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMAX.Disruptor.Sample
{
    //產品
    public class MyProduct
    {
        public static Func<MyProduct> FactoryFunc = () => new MyProduct();
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
