using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Task_2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TestsSet testsContainer = new TestsSet(new JsonReader());
            await testsContainer.RunAll();
        }
    }
}
