using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using LMAX.Disruptor.Sample;

namespace LMAX.MyDisruptor.Sample
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Program");
            var program = new BasicDisruptorSample();
            program.Run();
            Console.WriteLine("End Program");
            Console.ReadKey();

        }
    }

    


#if false //這版本也不是正規 Producer / Consumer 的模式 (僅有Producer)
    //from https://stackoverflow.com/questions/8860684/disruptor-net-example
    public sealed class ValueEntry
    {
        public long Value { get; set; }

        public ValueEntry()
        {
            Console.WriteLine("New ValueEntry created");
        }
    }

    //
    public class ValueAdditionHandler : IEventHandler<ValueEntry>
    {

        public void OnEvent(ValueEntry data, long sequence, bool endOfBatch)
        {
            Console.WriteLine($"OnEvent:  Value = {data.Value} sequence = {sequence} (endOfBatch ={endOfBatch}" );
        }
    }

    class Program
    {
        private static readonly Random _random = new Random();
        private static readonly int _ringSize = 16;  // Must be multiple of 2

        static void Main(string[] args)
        {
            var disruptor = new  Disruptor.Dsl.Disruptor<ValueEntry>(
                () => new ValueEntry(), _ringSize, TaskScheduler.Default);

            disruptor.HandleEventsWith(new ValueAdditionHandler());

            var ringBuffer = disruptor.Start();

            while (true)
            {
                long sequenceNo = ringBuffer.Next();

                ValueEntry entry = ringBuffer[sequenceNo];

                entry.Value = _random.Next();

                //生產者產生資料
                ringBuffer.Publish(sequenceNo);

                Console.WriteLine("Published entry {0}, value {1}", sequenceNo, entry.Value);

                Thread.Sleep(1000);
            }
        }
    }

#endif
}
