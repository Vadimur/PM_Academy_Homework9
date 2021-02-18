using System;
using System.Linq;

namespace Task_1.Domain
{
    public class PrimesWorker : IPrimesWorker
    {
        public bool IsPrime(int number)
        {
            if (number < 2)
            {
                return false;
            }
            
            return Enumerable.Range(2, (int) Math.Sqrt(number) - 1)
                             .All(x => number % x != 0);
        }

        public int[] FindPrimesInRange(int from, int to)
        {
            if (from < 0 && to < 0)
            {
                return new int[] {};
            }
            
            from = from < 2 ? 2 : from;
            if (to <= from)
                return new int[] {};
            
            int[] primes = Enumerable.Range(from, to - from + 1)
                .AsParallel()
                .AsOrdered()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .Where(IsPrime).ToArray();
            
            return primes;
        }
    }
}