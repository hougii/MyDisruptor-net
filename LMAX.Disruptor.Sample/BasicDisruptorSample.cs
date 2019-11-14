using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using LMAX.MyDisruptor.Sample;

namespace LMAX.Disruptor.Sample
{
    public class BasicDisruptorSample:IMyDisruptor
    {
        /*ref github sample: https://github.com/disruptor-net/Disruptor-net
                            https://github.com/disruptor-net/Disruptor-net/tree/master/src/Disruptor.PerfTests/WorkHandler
             */

        /*
         * 簡單體驗Disruptor的運作模式：一個RingBuffer，一個Producer(產生資料)，三個Cumsumer(處理資料)的模式
         * my note:
         *  a. worker pool:放ringBuffer及Workers
         *  b. 透過RingBuffer取得格子，存放進去後，執行Publish，即觸發相關Event做事。
         */
        
        private const int _numWorkers = 3; //Worker數
        private const int _bufferSize = 1 * 8; //給較小的，僅8個Buffer
        private const long _iterations = 100L; //* 1000L * 100L; //執行次數

        //Ring Buffer Obj(最重要就是Init出RingBuffer)
        private readonly RingBuffer<MyProduct> _ringBuffer = RingBuffer<MyProduct>.CreateSingleProducer(
            MyProduct.FactoryFunc,
            _bufferSize,
            new YieldingWaitStrategy());


        //Worker Pool
        private readonly WorkerPool<MyProduct> _workerPool;


        public BasicDisruptorSample()
        {
            ConsumerWorkHandler[] _workers = new ConsumerWorkHandler[_numWorkers];
            //Init Worker
            for (int i = 0; i < _numWorkers; i++)
            {
                _workers[i] = new ConsumerWorkHandler("Cust" + i);
            }
            //將RingBuffer加入Worker Pool
            _workerPool = new WorkerPool<MyProduct>(
                _ringBuffer,
                _ringBuffer.NewBarrier(),
                new ExceptionHandlerWrapper<MyProduct>(),
                _workers);

            //各自worker 的Sequence
            _ringBuffer.AddGatingSequences(_workerPool.GetWorkerSequences());
        }

        public void Run()
        {
            //產生Producer
            var ringBuffer = _workerPool.Start(new BasicExecutor(TaskScheduler.Default));
            
            //開始填值
            for (long i = 0; i < _iterations; i++)
            {
                //從RingBuffer取得序列號
                var sequence = ringBuffer.Next();
                Console.WriteLine($"sequence={sequence} Cursor={ringBuffer.Cursor} GetRemainingCapacity={ringBuffer.GetRemainingCapacity()}" );
                //修改該序列號下的Model屬性
                var product = ringBuffer[sequence];
                product.Name = "Prod"+sequence;
                product.Value = i.ToString();
                //再推送回RingBuffer (會觸發相邊Event)
                ringBuffer.Publish(sequence);
                Thread.Sleep(500);
            }
        }
    }

    public class ConsumerWorkHandler : IWorkHandler<MyProduct>
    {
        private string _workName;
        private Random _random = new Random();
        public ConsumerWorkHandler(string workName)
        {
            _workName = workName;
        }

        public void OnEvent(MyProduct evt)
        {
            //隨機睡一段時間(表示處理程序的耗時)
            int sleepTime = _random.Next() % 2000;
            Console.WriteLine($"{_workName} execute product ={evt.Name} {evt.Value} SleepTime={sleepTime}");
            Thread.Sleep(sleepTime);
        }
    }
}
