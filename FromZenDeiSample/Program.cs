using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;

namespace FromZenDeiSample
{
    //http://www.zendei.com/article/4362.html
    //這引用的版本太久了，所有Workers / Producer 的Class 都不對

    class Program
    {

        static void Main(string[] args)
        {
        }

#if false
       public static long PrePkgInCount = 0;
        public static long PrePkgOutCount = 0;
        public static long PkgInCount = 0;
        public static long PkgOutCount = 0;
        static ConcurrentDictionary<string, string> InCache = new ConcurrentDictionary<string, string>();
        static ConcurrentDictionary<string, string> OutCache = new ConcurrentDictionary<string, string>();
        private static long Seconds;

        static void Main(string[] args)
        {
            Workers<Product> workers = new Workers<Product>(
            new List<IWorkHandler<Product>>() {new WorkHandler(), new WorkHandler()});

            Producer<Product> producerWorkers = workers.CreateOneProducer();
            Producer<Product> producerWorkers1 = workers.CreateOneProducer();

            workers.Start();
            Task.Run(delegate
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    Seconds++;
                    long intemp = PkgInCount;
                    long outemp = PkgOutCount;
                    Console.WriteLine(
                        $"In ops={intemp - PrePkgInCount},out ops={outemp - PrePkgOutCount},inCacheCount={InCache.Count},OutCacheCount={OutCache.Count},RunningTime={Seconds}");
                    PrePkgInCount = intemp;
                    PrePkgOutCount = outemp;
                }

            });
            Task.Run(delegate { Run(producerWorkers); });
            Task.Run(delegate { Run(producerWorkers); });
            Task.Run(delegate { Run(producerWorkers1); });
            Console.Read();

        }

        public static void Run(Producer<Product> producer)
        {
            for (int i = 0; i < int.MaxValue; i++)
            {

                var obj = producer.Enqueue();
                CheckRelease(obj as Product);
                obj.Commit();
            }
        }

        public static  void CheckRelease(Product publisher)
        {
            Interlocked.Increment(ref PkgInCount);
            return; //不檢查正確性
            publisher.Guid = Guid.NewGuid().ToString();
            InCache.TryAdd(publisher.Guid, string.Empty);
          
        }

        public static void UpdateCacheByOut(string guid)
        {
            Interlocked.Increment(ref PkgOutCount);
            if (guid != null)
                if (InCache.ContainsKey(guid))
                {
                    string str;
                    InCache.TryRemove(guid, out str);
                }
                else
                {
                    OutCache.TryAdd(guid, string.Empty);
                }

        }
        /// <summary>
        /// 產品/繼承生產者
        /// </summary>
        public class Product :  Producer<Product>
        {
            //產品包含的屬下隨便定義,無要求,只需要繼承自生產者就行了
            public long Value { get; set; }
            public string Guid { get; set; }
        }

        /// <summary>
        /// 消費處理對象
        /// </summary>
        public class WorkHandler : IWorkHandler<Product>
        {
         
            public void OnEvent(Product @event)
            {

                Program.UpdateCacheByOut(@event.Guid);
                //收到產品,在這裡寫處理代碼

            }

        }
#endif
    }
}
